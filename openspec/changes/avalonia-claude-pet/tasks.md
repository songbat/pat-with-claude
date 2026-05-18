---
name: avalonia-claude-pet
status: proposed
created: 2026-05-18
---

# Tasks: AvaloniaClaudePet

## Phase 1: Project Setup & Core Infrastructure

### Task 1.1: Initialize Avalonia Project
- [x] Create AvaloniaClaudePet solution and project with `dotnet new avalonia.app`
- [x] Configure .NET 8 target framework
- [x] Set up project structure (Views, ViewModels, Services, Models directories)
- [x] Add NuGet dependencies: Avalonia (11.x), Avalonia.Desktop, Avalonia.Themes.Fluent
- **Estimate**: 30 min

### Task 1.2: Pet State Machine
- [x] Define `PetState` enum (Idle, Thinking, Working, Error, Success, Waiting)
- [x] Define `PetTrigger` enum (PromptSubmit, ToolStart, ToolEnd, ToolFailure, Stop, StopFailure, NotificationIdle, NotificationPermission, SubagentStart, SubagentStop, SessionEnd)
- [x] Implement `PetStateMachine` with state transitions per the mapping table
- [x] Add transition timer for temporary states (Error: 2s, Success: 3s)
- [x] Unit tests for all valid transitions
- **Estimate**: 1.5 hours

### Task 1.3: Embedded HTTP Server
- [x] Implement `HookHttpServer` using ASP.NET Core Minimal API
- [x] Create all POST endpoints matching the hook event routing
- [x] Create `HookPayload` model for deserialization
- [x] Implement health check endpoint (`GET /health`)
- [x] Implement port discovery (try 12345-12350)
- [x] Run server on background thread, dispatch events to UI thread
- [x] Integration tests for all endpoints
- **Estimate**: 2 hours

## Phase 2: Pet UI

### Task 2.1: Pet Expression Assets
- [x] Create or source expression images (procedural drawing via DrawingContext instead of PNG)
- [x] Required expressions: idle, thinking, working, success, error, waiting
- [x] ~~Add HiDPI variants (256x256)~~ N/A - procedural rendering scales with DPI
- [x] ~~Add assets to `Assets/Expressions/`~~ N/A - no asset files needed
- **Estimate**: 1 hour (depends on asset sourcing)

### Task 2.2: Pet Window & Rendering
- [x] Create `PetWindow` (transparent, borderless, always-on-top, draggable)
- [x] Create `PetControl` with image binding to current expression
- [x] Implement `PetViewModel` with observable `CurrentState` and `CurrentExpression`
- [x] Add cross-fade transition animation (200ms) between expressions
- [x] Wire `PetStateMachine` → `PetViewModel` → `PetControl`
- [x] Test drag behavior and window positioning
- **Estimate**: 2 hours

### Task 2.3: Notification Bubble
- [x] Create `BubbleWindow` (transparent, borderless, always-on-top overlay)
- [x] Create `BubbleViewModel` with message and visibility state
- [x] Implement slide-in / slide-out animations (300ms / 200ms)
- [x] Implement auto-dismiss timer (10s)
- [x] Wire `NotificationService` to show/hide bubble on hook events
- [x] Position bubble relative to pet window location
- [x] Handle replacement (new notification dismisses existing)
- **Estimate**: 2 hours

## Phase 3: Integration

### Task 3.1: Hooks Configuration Service
- [x] Implement `HookConfigService` to read/write `~/.claude/settings.json`
- [x] Generate hooks configuration with correct port
- [x] Handle SessionStart `command` hook (curl/Invoke-WebRequest)
- [x] Detect existing hooks and merge (don't overwrite unrelated settings)
- [x] Add cross-platform path resolution for settings file
- [x] Add "Configure Hooks" option in system tray menu
- **Estimate**: 1.5 hours

### Task 3.2: System Tray Integration
- [x] Add `TrayIcon` with context menu
- [x] Menu items: Show Pet, Configure Hooks, Port Info, Quit
- [x] Minimize to tray on window close (don't exit)
- [x] Show notification on first run guiding user to configure hooks
- **Estimate**: 1 hour

### Task 3.3: End-to-End Integration
- [x] Wire all components: HTTP server → state machine → view model → UI
- [x] Verify hook events flow from Claude Code to pet expressions
- [x] Verify notification bubbles appear on idle_prompt and permission_prompt
- [x] Test multi-session scenarios (session start/end cycling)
- [x] Test error recovery (server restart, port change)
- **Estimate**: 1.5 hours

## Phase 4: Polish & Distribution

### Task 4.1: Cross-Platform Testing
- [x] Test on Windows 10/11
- [ ] Test on macOS (Intel and Apple Silicon)
- [ ] Test on Linux (Ubuntu, Fedora)
- [x] Verify transparency, always-on-top, and tray behavior on each OS (Windows only)
- **Estimate**: 2 hours

### Task 4.2: Packaging
- [x] Configure `dotnet publish` for single-file self-contained builds
- [x] Windows: MSIX or portable exe
- [ ] macOS: .app bundle
- [ ] Linux: AppImage or tar.gz
- **Estimate**: 1.5 hours

### Task 4.3: README & Documentation
- [x] Write README with setup instructions
- [x] Document hooks configuration process
- [ ] Add screenshots of pet expressions
- [x] Document troubleshooting (port conflicts, hooks not firing)
- **Estimate**: 1 hour

## Total Estimate

| Phase | Hours |
|-------|-------|
| Phase 1: Infrastructure | 4 |
| Phase 2: Pet UI | 5 |
| Phase 3: Integration | 4 |
| Phase 4: Polish | 4.5 |
| **Total** | **17.5** |
