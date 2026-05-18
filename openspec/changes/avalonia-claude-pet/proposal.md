---
name: avalonia-claude-pet
status: proposed
created: 2026-05-18
---

# AvaloniaClaudePet - Claude Code Desktop Pet

## Problem

Claude Code CLI users frequently switch between terminal and other windows while waiting for AI responses. When Claude Code needs user interaction (permission approval, idle timeout), there is no visual notification outside the terminal. Users miss prompts, causing wasted idle time.

## Proposal

Build a cross-platform desktop electronic pet application that sits alongside Claude Code CLI and provides two core capabilities:

1. **Expression Mapping** - The pet displays different facial expressions based on Claude Code's current state (thinking, success, error, idle, etc.)
2. **Notification Bubbles** - When Claude Code needs user interaction, the pet displays a reminder bubble telling the user to return to the CLI

The integration uses Claude Code's official **Hooks system** (HTTP hooks) to receive structured event data in real-time.

## Scope

### In Scope

- Avalonia UI desktop application (Windows, macOS, Linux)
- Embedded HTTP server receiving Claude Code hook events
- Pet character with state-driven expression animations
- Notification bubble system for permission and idle prompts
- System tray integration for background operation
- Hook configuration generation / auto-setup
- Cross-platform build and packaging

### Out of Scope

- Voice notifications or sound effects
- Multiple pet characters / customization
- Plugin system for other AI tools
- Mobile companion app
- Cloud synchronization

## Technical Approach

**Option A: Claude Code Hooks (chosen)**

The app embeds a lightweight HTTP server. Claude Code's `hooks` configuration sends POST requests with structured JSON event data to the server on every lifecycle event. The pet maps these events to expressions and triggers notifications.

Key trade-off: Requires one-time hooks configuration in `~/.claude/settings.json`, but provides structured, stable, official API coverage of all events including "needs user input" detection.

## Architecture Overview

```
Claude Code CLI                          Pet App (Avalonia)
┌──────────────────┐                     ┌──────────────────────┐
│  Hooks System    │──HTTP POST JSON──▶ │  Embedded HTTP Server │
│  (~/.claude/     │   localhost:12345   │  (per-event endpoint) │
│   settings.json) │                     └──────────┬───────────┘
└──────────────────┘                                │
                                          ┌─────────▼──────────┐
                                          │  State Machine      │
                                          │  (hook → pet state) │
                                          └─────────┬──────────┘
                                                    │
                                    ┌───────────────┼───────────────┐
                                    ▼               ▼               ▼
                              ┌──────────┐  ┌────────────┐  ┌───────────┐
                              │Expression│  │Notification│  │ System    │
                              │ Renderer │  │  Bubbles   │  │   Tray    │
                              └──────────┘  └────────────┘  └───────────┘
```

## Success Criteria

- Pet correctly displays expressions for all mapped Claude Code states
- Notification bubbles appear within 1 second of permission/idle events
- App runs on Windows, macOS, and Linux without platform-specific code paths
- Minimal resource usage (< 100MB RAM, < 5% CPU when idle)
