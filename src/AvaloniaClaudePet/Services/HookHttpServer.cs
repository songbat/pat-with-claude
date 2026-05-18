using System.Net;
using System.Text.Json;
using AvaloniaClaudePet.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AvaloniaClaudePet.Services;

public class HookHttpServer : IAsyncDisposable
{
    private WebApplication? _app;
    private int _port;
    private readonly PetStateMachine _stateMachine;
    private readonly Action<NotificationInfo>? _onNotification;
    private readonly Action? _dismissNotification;
    private readonly Action<string, string?>? _onStatusUpdate;
    private bool _disposed;

    public int Port => _port;
    public bool IsRunning => _app != null;

    public HookHttpServer(PetStateMachine stateMachine,
        Action<NotificationInfo>? onNotification = null,
        Action? dismissNotification = null,
        Action<string, string?>? onStatusUpdate = null)
    {
        _stateMachine = stateMachine;
        _onNotification = onNotification;
        _dismissNotification = dismissNotification;
        _onStatusUpdate = onStatusUpdate;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        _port = FindAvailablePort(12345, 12350);

        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls($"http://localhost:{_port}");
        builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 1024 * 1024);

        _app = builder.Build();

        _app.MapGet("/health", () => Results.Json(new HealthResponse { Status = "healthy", Port = _port }));

        _app.MapPost("/hooks/session-start", (HookPayload payload) => HandleStatusHook(PetTrigger.SessionStart, "analyzing", payload));
        _app.MapPost("/hooks/prompt-submit", (HookPayload payload) => HandleStatusHook(PetTrigger.PromptSubmit, "analyzing", payload));
        _app.MapPost("/hooks/pre-tool-use", (HookPayload payload) => HandleToolHook(PetTrigger.ToolStart, payload));
        _app.MapPost("/hooks/post-tool-use", (HookPayload payload) => HandleStatusHook(PetTrigger.ToolEnd, "processing", payload));
        _app.MapPost("/hooks/tool-failure", (HookPayload payload) => HandleStatusHook(PetTrigger.ToolFailure, "failed", payload));
        _app.MapPost("/hooks/stop", (HookPayload payload) => HandleStatusHook(PetTrigger.Stop, "done", payload));
        _app.MapPost("/hooks/stop-failure", (HookPayload payload) => HandleStatusHook(PetTrigger.StopFailure, "failed", payload));
        _app.MapPost("/hooks/subagent-start", (HookPayload payload) => HandleStatusHook(PetTrigger.SubagentStart, "delegating", payload));
        _app.MapPost("/hooks/subagent-stop", (HookPayload payload) => HandleStatusHook(PetTrigger.SubagentStop, "processing", payload));
        _app.MapPost("/hooks/session-end", (HookPayload payload) => HandleStatusHook(PetTrigger.SessionEnd, "", payload));

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

    private IResult HandleStatusHook(PetTrigger trigger, string statusKey, HookPayload payload)
    {
        _dismissNotification?.Invoke();
        _onStatusUpdate?.Invoke(statusKey, null);
        return HandleHook(trigger, payload);
    }

    private IResult HandleToolHook(PetTrigger trigger, HookPayload payload)
    {
        _dismissNotification?.Invoke();
        var toolName = ExtractToolName(payload);
        _onStatusUpdate?.Invoke("processing", toolName);
        return HandleHook(trigger, payload);
    }

    private static string? ExtractToolName(HookPayload payload)
    {
        try
        {
            if (payload.Input.HasValue && payload.Input.Value.TryGetProperty("tool_name", out var toolNameEl))
            {
                return toolNameEl.GetString();
            }
        }
        catch { }
        return null;
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
        throw new InvalidOperationException($"No available port found in range {startPort}-{endPort}");
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            if (_app != null) await _app.DisposeAsync();
        }
    }
}
