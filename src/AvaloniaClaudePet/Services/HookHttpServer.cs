using System.Net;
using System.Text.Json;
using AvaloniaClaudePet.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AvaloniaClaudePet.Services;

public class HookHttpServer : IDisposable
{
    private WebApplication? _app;
    private int _port;
    private readonly PetStateMachine _stateMachine;
    private readonly Action<NotificationInfo>? _onNotification;
    private bool _disposed;

    public int Port => _port;
    public bool IsRunning => _app != null;

    public HookHttpServer(PetStateMachine stateMachine, Action<NotificationInfo>? onNotification = null)
    {
        _stateMachine = stateMachine;
        _onNotification = onNotification;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        _port = FindAvailablePort(12345, 12350);

        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls($"http://localhost:{_port}");

        _app = builder.Build();

        _app.MapGet("/health", () => Results.Json(new HealthResponse { Status = "healthy", Port = _port }));

        _app.MapPost("/hooks/session-start", (HookPayload payload) => HandleHook(PetTrigger.SessionStart, payload));
        _app.MapPost("/hooks/prompt-submit", (HookPayload payload) => HandleHook(PetTrigger.PromptSubmit, payload));
        _app.MapPost("/hooks/pre-tool-use", (HookPayload payload) => HandleHook(PetTrigger.ToolStart, payload));
        _app.MapPost("/hooks/post-tool-use", (HookPayload payload) => HandleHook(PetTrigger.ToolEnd, payload));
        _app.MapPost("/hooks/tool-failure", (HookPayload payload) => HandleHook(PetTrigger.ToolFailure, payload));
        _app.MapPost("/hooks/stop", (HookPayload payload) => HandleHook(PetTrigger.Stop, payload));
        _app.MapPost("/hooks/stop-failure", (HookPayload payload) => HandleHook(PetTrigger.StopFailure, payload));
        _app.MapPost("/hooks/subagent-start", (HookPayload payload) => HandleHook(PetTrigger.SubagentStart, payload));
        _app.MapPost("/hooks/subagent-stop", (HookPayload payload) => HandleHook(PetTrigger.SubagentStop, payload));
        _app.MapPost("/hooks/session-end", (HookPayload payload) => HandleHook(PetTrigger.SessionEnd, payload));

        _app.MapPost("/hooks/notification/idle", (HookPayload payload) =>
        {
            _onNotification?.Invoke(new NotificationInfo(NotificationType.IdlePrompt, "Claude is waiting for you!"));
            return HandleHook(PetTrigger.NotificationIdle, payload);
        });

        _app.MapPost("/hooks/notification/permission", (HookPayload payload) =>
        {
            _onNotification?.Invoke(new NotificationInfo(NotificationType.PermissionPrompt, "Claude needs your approval!"));
            return HandleHook(PetTrigger.NotificationPermission, payload);
        });

        await _app.StartAsync(ct);
    }

    private IResult HandleHook(PetTrigger trigger, HookPayload payload)
    {
        _stateMachine.Transition(trigger);
        return Results.Json(new { status = "ok" });
    }

    public async Task StopAsync()
    {
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
            _app = null;
        }
    }

    private static int FindAvailablePort(int startPort, int endPort)
    {
        for (int port = startPort; port <= endPort; port++)
        {
            try
            {
                var listener = new HttpListener();
                listener.Prefixes.Add($"http://localhost:{port}/");
                listener.Start();
                listener.Stop();
                return port;
            }
            catch
            {
                continue;
            }
        }
        return startPort;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _app?.DisposeAsync().AsTask().Wait();
        }
    }
}
