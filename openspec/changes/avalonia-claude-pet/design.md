---
name: avalonia-claude-pet
status: proposed
created: 2026-05-18
---

# Design: AvaloniaClaudePet

## Technology Stack

| Layer         | Technology              | Reason                          |
|---------------|-------------------------|---------------------------------|
| UI Framework  | Avalonia UI 11.x        | Cross-platform, .NET, XAML      |
| Runtime       | .NET 8 (LTS)           | Performance, cross-platform     |
| HTTP Server   | ASP.NET Core Minimal API| Lightweight, built-in           |
| JSON          | System.Text.Json        | Built-in, high performance      |
| Animation     | Avalonia Storyboards    | Native expression transitions   |
| System Tray   | Avalonia TrayIcon       | Cross-platform tray support     |
| Build         | dotnet publish          | Single-file, self-contained     |

## Project Structure

```
AvaloniaClaudePet/
├── src/
│   ├── AvaloniaClaudePet.sln
│   ├── AvaloniaClaudePet/
│   │   ├── App.axaml                    # Application entry
│   │   ├── App.axaml.cs
│   │   ├── Views/
│   │   │   ├── PetWindow.axaml          # Transparent pet window
│   │   │   ├── PetWindow.axaml.cs
│   │   │   ├── BubbleWindow.axaml       # Notification bubble overlay
│   │   │   └── BubbleWindow.axaml.cs
│   │   ├── Controls/
│   │   │   ├── PetControl.axaml         # Pet character rendering
│   │   │   └── PetControl.axaml.cs
│   │   ├── Assets/
│   │   │   └── Expressions/             # Expression image assets
│   │   │       ├── idle.png
│   │   │       ├── thinking.png
│   │   │       ├── success.png
│   │   │       ├── error.png
│   │   │       ├── waiting.png
│   │   │       └── ...
│   │   ├── ViewModels/
│   │   │   ├── PetViewModel.cs          # Pet state & expression logic
│   │   │   └── BubbleViewModel.cs       # Notification bubble state
│   │   ├── Services/
│   │   │   ├── HookHttpServer.cs        # Embedded HTTP server
│   │   │   ├── PetStateMachine.cs       # Event → state mapping
│   │   │   ├── NotificationService.cs   # Bubble display logic
│   │   │   └── HookConfigService.cs     # Hook config setup helper
│   │   ├── Models/
│   │   │   ├── HookEvent.cs             # Hook event DTOs
│   │   │   └── PetState.cs              # Pet state enum
│   │   └── Program.cs
│   └── AvaloniaClaudePet.Tests/
│       ├── PetStateMachineTests.cs
│       └── HookHttpServerTests.cs
├── openspec/
│   ├── config.yaml
│   ├── changes/
│   └── specs/
└── README.md
```

## Hook Event Mapping

### State Machine

```
                    ┌─────────┐
         ┌─────────│  Idle   │◀──────────────────────┐
         │         └────┬────┘                       │
         │              │ UserPromptSubmit            │
         │              ▼                             │
         │         ┌──────────┐                       │
         │    ┌───▶│ Thinking │◀──┐                   │
         │    │    └────┬─────┘   │                   │
         │    │         │         │ PreToolUse        │
         │    │         ▼         │ (tool call start) │
         │    │    ┌─────────┐   │                   │
         │    │    │Working  │───┘                   │
         │    │    │(Tool)   │                       │
         │    │    └────┬────┘                       │
         │    │         │                            │
         │    │    ┌────┴────┐                       │
         │    │    ▼         ▼                       │
         │    │ Success   Error                      │
         │    │    │    (Failure)                    │
         │    │    │         │                       │
         │    │    ▼         ▼                       │
         │    │  ┌──────────────┐                    │
         │    │  │   Stop /     │────────────────────┘
         │    │  │ StopFailure  │    (session ends or
         │    │  └──────────────┘     back to idle)
         │    │         │
         │    │         ▼
         │    │   ┌──────────┐
         │    │   │ Waiting  │─── Notification bubble
         │    │   │ (needs   │    (permission_prompt /
         │    │   │  input)  │     idle_prompt)
         │    │   └────┬─────┘
         │    │        │ user returns to CLI
         │    └────────┘
         │
         │  SessionEnd
         └──────────────────────────────────────────▶Idle
```

### Event → Expression Mapping Table

| Hook Event           | Pet Expression | Description                          |
|----------------------|----------------|--------------------------------------|
| SessionStart         | `idle`         | Pet wakes up, greets user            |
| UserPromptSubmit     | `thinking`     | Pet enters thinking animation        |
| PreToolUse           | `working`      | Pet shows "busy working" animation   |
| PostToolUse          | `working`      | Stays in working state               |
| PostToolUseFailure   | `error`        | Pet shows concern/error expression   |
| Notification(idle)   | `waiting`      | Pet looks at user + bubble: "Come back!" |
| Notification(perm)   | `waiting`      | Pet waves + bubble: "Need approval!" |
| Stop                 | `success`      | Pet celebrates / happy expression    |
| StopFailure          | `error`        | Pet shows sad/concerned expression   |
| SubagentStart        | `thinking`     | Pet enters deeper thinking mode      |
| SubagentStop         | `working`      | Returns to working state             |
| SessionEnd           | `idle`         | Pet goes to sleep / idle             |

## HTTP Server Design

### Endpoints

```
POST /hooks/prompt-submit        → UserPromptSubmit event
POST /hooks/pre-tool-use         → PreToolUse event
POST /hooks/post-tool-use        → PostToolUse event
POST /hooks/tool-failure         → PostToolUseFailure event
POST /hooks/notification/idle    → Notification (idle_prompt)
POST /hooks/notification/permission → Notification (permission_prompt)
POST /hooks/stop                 → Stop event
POST /hooks/stop-failure         → StopFailure event
POST /hooks/subagent-start       → SubagentStart event
POST /hooks/subagent-stop        → SubagentStop event
POST /hooks/session-end          → SessionEnd event
```

### SessionStart (command hook)

Since SessionStart only supports `command` and `mcp_tool` hook types (not HTTP), the app will:
1. Register a `command` hook that calls `curl http://localhost:12345/hooks/session-start`
2. The command hook writes session metadata to the HTTP endpoint

### Hook Payload Format

All endpoints receive POST with JSON body. Claude Code hooks send:

```json
{
  "session_id": "abc123",
  "hook_event_name": "UserPromptSubmit",
  "input": {
    "prompt": "user's prompt text (for UserPromptSubmit)"
  }
}
```

Additional fields vary by event type (tool_name, tool_input, exit_code, etc.).

### Server Implementation

```csharp
// Minimal API pattern
var builder = WebApplication.CreateBuilder();
builder.WebHost.UseUrls("http://localhost:12345");
var app = builder.Build();

app.MapPost("/hooks/prompt-submit", (HookPayload payload) =>
{
    stateMachine.Transition(PetTrigger.PromptSubmit, payload);
    return Results.Ok();
});
// ... other endpoints

app.Run();
```

Runs on a background thread alongside Avalonia UI. Uses `IServiceCollection` for DI.

## Pet Window Design

### Transparency & Always-on-Top

```
┌─────────────────────────────────────────────┐
│  Desktop / Other Windows                    │
│                                             │
│         ┌──────────────┐                    │
│         │  🐱 Pet      │  ← Transparent    │
│         │  Character   │    window,         │
│         │  (128x128)   │    always-on-top   │
│         └──────────────┘                    │
│                                             │
│  ┌──────────────────┐                       │
│  │ 💬 Claude needs  │  ← Notification      │
│  │ your approval!   │    bubble (overlay)   │
│  └──────────────────┘                       │
│                                             │
└─────────────────────────────────────────────┘
```

- `PetWindow`: Transparent background, `WindowStyle="None"`, `Topmost="True"`, draggable
- `BubbleWindow`: Separate transparent overlay for notification bubbles, auto-dismiss after 10s or on next event

### Expression Rendering

Expressions use image assets (PNG with transparency). The `PetControl` switches the displayed image based on `PetState`:

```xml
<Image Source="{Binding CurrentExpression}" 
       Width="128" Height="128"
       RenderOptions.BitmapInterpolationMode="HighQuality"/>
```

Transition animations use Avalonia `Storyboard` with cross-fade between expressions.

## Hooks Configuration

### Settings Schema

The app generates or reads `~/.claude/settings.json` hooks section:

```json
{
  "hooks": {
    "SessionStart": [{
      "hooks": [{
        "type": "command",
        "command": "curl -s -X POST http://localhost:12345/hooks/session-start -H 'Content-Type: application/json' -d '{\"session_id\":\"$CLAUDE_SESSION_ID\",\"hook_event_name\":\"SessionStart\"}'"
      }]
    }],
    "UserPromptSubmit": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/prompt-submit" }]
    }],
    "PreToolUse": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/pre-tool-use" }]
    }],
    "PostToolUse": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/post-tool-use" }]
    }],
    "PostToolUseFailure": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/tool-failure" }]
    }],
    "Notification": [
      {
        "matcher": "idle_prompt",
        "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/notification/idle" }]
      },
      {
        "matcher": "permission_prompt",
        "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/notification/permission" }]
      }
    ],
    "Stop": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/stop" }]
    }],
    "StopFailure": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/stop-failure" }]
    }],
    "SubagentStart": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/subagent-start" }]
    }],
    "SubagentStop": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/subagent-stop" }]
    }],
    "SessionEnd": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/session-end" }]
    }]
  }
}
```

### Setup Flow

1. App starts → HTTP server listens on port 12345
2. `HookConfigService` checks if hooks are already configured
3. If not configured, prompts user to auto-configure (merges into existing settings)
4. App provides a "Configure Hooks" button in system tray menu as fallback

## Cross-Platform Considerations

| Concern         | Windows       | macOS              | Linux              |
|-----------------|---------------|--------------------|--------------------|
| Transparency    | `AllowsTransparency` | `AllowsTransparency` | `AllowsTransparency` (X11/Wayland) |
| Always-on-top   | `Topmost`     | `NSLevel` via Avalonia | `_NET_WM_STATE_ABOVE` |
| System Tray     | `TrayIcon`    | `TrayIcon` (menu bar) | `TrayIcon` (libappindicator) |
| HTTP Server     | `HttpListener` / Kestrel | Kestrel | Kestrel |
| `curl` command  | May need fallback | Native | Native |

### Port Conflict Handling

- Default port: 12345
- If port is occupied, try 12346-12350
- Store selected port in `HookConfigService` and update hooks config
- On startup, verify hooks point to correct port

## Notification Bubble Design

### Bubble Behavior

```
Timeline:
─────────────────────────────────────────────────
  Event        Bubble Shows     Auto-dismiss    User returns
  Received     Immediately      (10s timeout)   (next event)
─────────────────────────────────────────────────
     ▲            ▲                ▲                ▲
     │            │                │                │
  Permission   Displayed       Dismiss if no      Dismiss on
  or Idle      with sound*     interaction        next event
  event                        within 10s
```

\* Sound is out of scope for v1.

### Bubble Content

| Event Type         | Bubble Text                     | Icon    |
|--------------------|---------------------------------|---------|
| idle_prompt        | "Claude is waiting for you!"    | ⏳      |
| permission_prompt  | "Claude needs your approval!"   | 🔐      |

Bubble appearance: rounded rectangle, semi-transparent background, slides in from pet position, fades out on dismiss.
