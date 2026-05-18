# AvaloniaClaudePet

A desktop pet for Claude Code that sits on your screen and reacts to Claude's activity in real-time.

## Features

- Desktop pet cat that changes expressions based on Claude Code state
- System tray integration with language toggle (Chinese/English)
- Notification bubbles for idle and permission prompts
- Status speech bubble showing current activity
- Procedurally drawn cat (no external assets needed)
- Auto-configures Claude Code hooks on first run

## Pet Expressions

| State | Expression | Description |
|-------|-----------|-------------|
| Idle | Gray cat, neutral face | Waiting for interaction |
| Thinking | Olive cat, thought bubbles | Claude is analyzing |
| Working | Blue cat, focused brows | Claude is running tools |
| Error | Red cat, X eyes, sad mouth | Something went wrong |
| Success | Green cat, closed eyes, blush | Task completed |
| Waiting | Yellow cat, big eyes, O mouth | Waiting for your answer |

## Prerequisites

- .NET 8 SDK
- Claude Code CLI
- Windows 10/11 (macOS/Linux support planned)

## Build & Run

```bash
cd src
dotnet build
dotnet run
```

## How It Works

The app starts an embedded HTTP server (port 12345-12350) and configures Claude Code hooks to send events to it. When Claude processes your requests, the pet reacts in real-time.

### Hook Events

| Hook | Pet Reaction |
|------|-------------|
| `prompt-submit` | Starts thinking |
| `pre-tool-use` | Shows working state |
| `post-tool-use` | Continues processing |
| `tool-failure` | Shows error briefly |
| `stop` | Shows "waiting for answer" |
| `stop-failure` | Shows error |
| `subagent-start` | Delegating indicator |
| `notification/idle` | Popup: "Claude is waiting for you!" |
| `notification/permission` | Popup: "Claude needs your approval!" |

### Hooks Configuration

On first run, the app automatically configures hooks in `~/.claude/settings.json`. The configuration merges with existing hooks without overwriting them.

To reconfigure, right-click the tray icon and select "Configure Hooks".

## System Tray Menu

- **Show Pet** - Show the desktop pet window
- **Configure Hooks** - Reconfigure Claude Code hooks
- **中/EN** - Toggle language between Chinese and English
- **Port: XXXXX** - Click to copy server URL to clipboard
- **Quit** - Exit the application

## Troubleshooting

### Hooks not firing
1. Check the port in the tray menu matches `~/.claude/settings.json`
2. Click "Configure Hooks" to reconfigure
3. Restart Claude Code after configuration changes

### Port conflicts
The app tries ports 12345-12350. If all are occupied, the app will fail to start. Close any conflicting processes.

### Pet window not visible
Right-click the tray icon and select "Show Pet". The window may have been moved off-screen.

## License

MIT
