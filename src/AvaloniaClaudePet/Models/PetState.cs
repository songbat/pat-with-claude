namespace AvaloniaClaudePet.Models;

public enum PetState
{
    Idle,
    Thinking,
    Working,
    Error,
    Success,
    Waiting
}

public enum PetTrigger
{
    PromptSubmit,
    ToolStart,
    ToolEnd,
    ToolFailure,
    Stop,
    StopFailure,
    NotificationIdle,
    NotificationPermission,
    SubagentStart,
    SubagentStop,
    SessionStart,
    SessionEnd
}
