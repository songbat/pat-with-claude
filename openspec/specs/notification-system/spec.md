---
name: notification-system
status: proposed
created: 2026-05-18
---

# Spec: Notification Bubbles

## Capability

Display visual notification bubbles when Claude Code CLI requires user interaction, specifically for permission approvals and idle waiting scenarios.

## Notification Triggers

| Notification Matcher   | Trigger Condition                          | Priority |
|------------------------|-------------------------------------------|----------|
| `idle_prompt`          | Claude Code is waiting for user input     | Medium   |
| `permission_prompt`    | Claude Code needs tool permission approval| High     |

## Hook Payload Examples

### idle_prompt

```json
{
  "session_id": "abc123",
  "hook_event_name": "Notification",
  "input": {}
}
```

### permission_prompt

```json
{
  "session_id": "abc123",
  "hook_event_name": "Notification",
  "input": {}
}
```

## Bubble Display Behavior

### Appearance

- Rounded rectangle, semi-transparent background (e.g., 85% opacity white)
- Positioned relative to pet character position (above or beside)
- Shadow effect for depth
- Contains icon + text message

### Content

| Type                | Icon | Message                         |
|---------------------|------|---------------------------------|
| idle_prompt         | ⏳   | "Claude is waiting for you!"    |
| permission_prompt   | 🔐   | "Claude needs your approval!"   |

### Show / Hide Rules

1. **Show**: Immediately on receiving notification hook event
2. **Auto-dismiss**: After 10 seconds if no user interaction
3. **Dismiss on event**: Any subsequent hook event (e.g., `UserPromptSubmit`) dismisses the bubble
4. **No stacking**: Only one bubble visible at a time; new notification replaces existing

### Animation

- **Show**: Slide-in from pet position + fade-in (300ms)
- **Hide**: Slide-out + fade-out (200ms)
- **Idle pulse**: Subtle pulsing animation while visible to draw attention

## Window Architecture

```
BubbleWindow (separate transparent overlay window)
├── Background: semi-transparent rounded rect
├── Icon (Image/TextBlock)
├── Message (TextBlock)
└── Close button (optional, small × in corner)
```

- `WindowStyle="None"`, `AllowsTransparency="True"`, `Topmost="True"`
- `ShowInTaskbar="False"` - does not appear in taskbar
- Position calculated relative to `PetWindow` location

## Interaction

- Clicking the bubble: optional future behavior (e.g., focus terminal window)
- v1: Bubble is display-only, no click interaction required
- Bubble does not steal focus from current application

## Threading

- Notification hooks arrive on HTTP server background thread
- UI updates marshaled to Avalonia UI thread via `Dispatcher.UIThread.Post()`
- Bubble show/hide must happen on UI thread
