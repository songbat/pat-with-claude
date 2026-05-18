using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AvaloniaClaudePet.Models;
using AvaloniaClaudePet.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AvaloniaClaudePet.Tests;

public class HookHttpServerTests : IAsyncLifetime
{
    private PetStateMachine _stateMachine = null!;
    private HookHttpServer _server = null!;
    private HttpClient _client = null!;
    private int _port;

    public async Task InitializeAsync()
    {
        _stateMachine = new PetStateMachine();
        _server = new HookHttpServer(_stateMachine);
        await _server.StartAsync();
        _port = _server.Port;
        _client = new HttpClient { BaseAddress = new Uri($"http://localhost:{_port}") };
    }

    public async Task DisposeAsync()
    {
        await _server.StopAsync();
        _client.Dispose();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var resp = await _client.GetAsync("/health");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("healthy", body.GetProperty("status").GetString());
        Assert.Equal(_port, body.GetProperty("port").GetInt32());
    }

    [Fact]
    public async Task PromptSubmit_TransitionsToThinking()
    {
        var payload = new HookPayload { SessionId = "test", HookEventName = "UserPromptSubmit" };
        var resp = await _client.PostAsJsonAsync("/hooks/prompt-submit", payload);
        resp.EnsureSuccessStatusCode();
        Assert.Equal(PetState.Thinking, _stateMachine.CurrentState);
    }

    [Fact]
    public async Task PreToolUse_TransitionsToWorking()
    {
        _stateMachine.Transition(PetTrigger.PromptSubmit);
        var payload = new HookPayload { SessionId = "test", HookEventName = "PreToolUse" };
        var resp = await _client.PostAsJsonAsync("/hooks/pre-tool-use", payload);
        resp.EnsureSuccessStatusCode();
        Assert.Equal(PetState.Working, _stateMachine.CurrentState);
    }

    [Fact]
    public async Task Stop_TransitionsToSuccess()
    {
        _stateMachine.Transition(PetTrigger.PromptSubmit);
        _stateMachine.Transition(PetTrigger.ToolStart);
        var payload = new HookPayload { SessionId = "test", HookEventName = "Stop" };
        var resp = await _client.PostAsJsonAsync("/hooks/stop", payload);
        resp.EnsureSuccessStatusCode();
        Assert.Equal(PetState.Success, _stateMachine.CurrentState);
    }

    [Fact]
    public async Task SessionEnd_TransitionsToIdle()
    {
        _stateMachine.Transition(PetTrigger.PromptSubmit);
        var payload = new HookPayload { SessionId = "test", HookEventName = "SessionEnd" };
        var resp = await _client.PostAsJsonAsync("/hooks/session-end", payload);
        resp.EnsureSuccessStatusCode();
        Assert.Equal(PetState.Idle, _stateMachine.CurrentState);
    }
}
