---
name: expression-mapping
status: proposed
created: 2026-05-18
---

# Spec: Expression Mapping

## Capability

Map Claude Code CLI lifecycle events to pet facial expressions in real-time, creating a visual representation of what the AI is doing.

## Events and Expressions

| Hook Event         | Pet State   | Expression Asset | Duration / Behavior          |
|--------------------|-------------|------------------|------------------------------|
| SessionStart       | `idle`      | idle.png         | Subtle idle animation (blink)|
| UserPromptSubmit   | `thinking`  | thinking.png     | Animated until next event    |
| PreToolUse         | `working`   | working.png      | Animated (tool use in progress) |
| PostToolUse        | `working`   | working.png      | Stay in working state        |
| PostToolUseFailure | `error`     | error.png        | Hold 2s then return to working |
| Stop               | `success`   | success.png      | Hold 3s then return to idle  |
| StopFailure        | `error`     | error.png        | Hold 3s then return to idle  |
| SubagentStart      | `thinking`  | thinking.png     | Deeper thinking animation    |
| SubagentStop       | `working`   | working.png      | Return to working            |
| SessionEnd         | `idle`      | idle.png         | Sleep / idle animation       |

## State Machine Rules

1. **Idle** is the default and terminal state
2. **Thinking** transitions to **Working** on first `PreToolUse`
3. **Working** stays active across multiple tool calls (PreToolUse → PostToolUse cycles)
4. **Error** is a temporary state - auto-returns to previous state after 2s
5. **Success** is a temporary state - auto-returns to **Idle** after 3s
6. **Waiting** (notification) overlays on current state, does not replace it

## Expression Asset Requirements

- Format: PNG with alpha transparency
- Size: 128x128px base, support 2x (256x256) for HiDPI
- Naming convention: `{state}.png` (e.g., `idle.png`, `thinking.png`)
- Optional: animated GIF or sprite sheet for each state (v2 consideration)

## Transition Animations

- Default transition: 200ms cross-fade between expressions
- Error state: slight shake animation on entry
- Success state: bounce animation on entry
- Thinking state: subtle bobbing animation while active

## Data Flow

```
Hook POST → HookHttpServer → PetStateMachine.Transition(trigger, payload)
                                   │
                                   ▼
                            PetViewModel.CurrentState (observable)
                                   │
                                   ▼
                            PetControl updates Image.Source
```

## Error Handling

- Unknown hook events are logged but ignored (no state change)
- Invalid JSON payloads return 400 Bad Request to hook caller
- If state machine receives out-of-order events, apply idempotent fallback (e.g., PostToolUse without PreToolUse → just stay in current state)
