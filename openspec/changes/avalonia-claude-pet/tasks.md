---
name: avalonia-claude-pet
status: proposed
created: 2026-05-18
---

# Tasks: AvaloniaClaudePet

## Phase 1: Project Setup & Core Infrastructure

### Task 1.1: Initialize Avalonia Project
- [ ] Create AvaloniaClaudePet solution and project with `dotnet new avalonia.app`
- [ ] Configure .NET 8 target framework
- [ ] Set up project structure (Views, ViewModels, Services, Models directories)
- [ ] Add NuGet dependencies: Avalonia (11.x), Avalonia.Desktop, Avalonia.Themes.Fluent
- **Estimate**: 30 min

### Task 1.2: Pet State Machine
- [ ] Define `PetState` enum (Idle, Thinking, Working, Error, Success, Waiting)
- [ ] Define `PetTrigger` enum (PromptSubmit, ToolStart, ToolEnd, ToolFailure, Stop, StopFailure, NotificationIdle, NotificationPermission, SubagentStart, SubagentStop, SessionEnd)
- [ ] Implement `PetStateMachine` with state transitions per the mapping table
- [ ] Add transition timer for temporary states (Error: 2s, Success: 3s)
- [ ] Unit tests for all valid transitions
- **Estimate**: 1.5 hours

### Task 1.3: Embedded HTTP Server
- [ ] Implement `HookHttpServer` using ASP.NET Core Minimal API
- [ ] Create all POST endpoints matching the hook event routing
- [ ] Create `HookPayload` model for deserialization
- [ ] Implement health check endpoint (`GET /health`)
- [ ] Implement port discovery (try 12345-12350)
- [ ] Run server on background thread, dispatch events to UI thread
- [ ] Integration tests for all endpoints
- **Estimate**: 2 hours

## Phase 2: Pet UI

### Task 2.1: Pet Expression Assets
- [ ] Create or source expression images (PNG, 128x128, transparent background)
- [ ] Required expressions: idle, thinking, working, success, error, waiting
- [ ] Add HiDPI variants (256x256)
- [ ] Add assets to `Assets/Expressions/` with Avalonia resource configuration
- **Estimate**: 1 hour (depends on asset sourcing)

### Task 2.2: Pet Window & Rendering
- [ ] Create `PetWindow` (transparent, borderless, always-on-top, draggable)
- [ ] Create `PetControl` with image binding to current expression
- [ ] Implement `PetViewModel` with observable `CurrentState` and `CurrentExpression`
- [ ] Add cross-fade transition animation (200ms) between expressions
- [ ] Wire `PetStateMachine` → `PetViewModel` → `PetControl`
- [ ] Test drag behavior and window positioning
- **Estimate**: 2 hours

### Task 2.3: Notification Bubble
- [ ] Create `BubbleWindow` (transparent, borderless, always-on-top overlay)
- [ ] Create `BubbleViewModel` with message and visibility state
- [ ] Implement slide-in / slide-out animations (300ms / 200ms)
- [ ] Implement auto-dismiss timer (10s)
- [ ] Wire `NotificationService` to show/hide bubble on hook events
- [ ] Position bubble relative to pet window location
- [ ] Handle replacement (new notification dismisses existing)
- **Estimate**: 2 hours

## Phase 3: Integration

### Task 3.1: Hooks Configuration Service
- [ ] Implement `HookConfigService` to read/write `~/.claude/settings.json`
- [ ] Generate hooks configuration with correct port
- [ ] Handle SessionStart `command` hook (curl/Invoke-WebRequest)
- [ ] Detect existing hooks and merge (don't overwrite unrelated settings)
- [ ] Add cross-platform path resolution for settings file
- [ ] Add "Configure Hooks" option in system tray menu
- **Estimate**: 1.5 hours

### Task 3.2: System Tray Integration
- [ ] Add `TrayIcon` with context menu
- [ ] Menu items: Show Pet, Configure Hooks, Port Info, Quit
- [ ] Minimize to tray on window close (don't exit)
- [ ] Show notification on first run guiding user to configure hooks
- **Estimate**: 1 hour

### Task 3.3: End-to-End Integration
- [ ] Wire all components: HTTP server → state machine → view model → UI
- [ ] Verify hook events flow from Claude Code to pet expressions
- [ ] Verify notification bubbles appear on idle_prompt and permission_prompt
- [ ] Test multi-session scenarios (session start/end cycling)
- [ ] Test error recovery (server restart, port change)
- **Estimate**: 1.5 hours

## Phase 4: Polish & Distribution

### Task 4.1: Cross-Platform Testing
- [ ] Test on Windows 10/11
- [ ] Test on macOS (Intel and Apple Silicon)
- [ ] Test on Linux (Ubuntu, Fedora)
- [ ] Verify transparency, always-on-top, and tray behavior on each OS
- **Estimate**: 2 hours

### Task 4.2: Packaging
- [ ] Configure `dotnet publish` for single-file self-contained builds
- [ ] Windows: MSIX or portable exe
- [ ] macOS: .app bundle
- [ ] Linux: AppImage or tar.gz
- **Estimate**: 1.5 hours

### Task 4.3: README & Documentation
- [ ] Write README with setup instructions
- [ ] Document hooks configuration process
- [ ] Add screenshots of pet expressions
- [ ] Document troubleshooting (port conflicts, hooks not firing)
- **Estimate**: 1 hour

## Total Estimate

| Phase | Hours |
|-------|-------|
| Phase 1: Infrastructure | 4 |
| Phase 2: Pet UI | 5 |
| Phase 3: Integration | 4 |
| Phase 4: Polish | 4.5 |
| **Total** | **17.5** |
