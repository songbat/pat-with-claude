using System.Collections.Immutable;
using AvaloniaClaudePet.Models;

namespace AvaloniaClaudePet.Services;

public class StateTransition
{
    public PetState From { get; }
    public PetTrigger Trigger { get; }
    public PetState To { get; }

    public StateTransition(PetState from, PetTrigger trigger, PetState to)
    {
        From = from;
        Trigger = trigger;
        To = to;
    }
}

public class PetStateMachine
{
    private PetState _currentState = PetState.Idle;
    private PetState _stateBeforeNotification = PetState.Idle;
    private readonly Dictionary<(PetState State, PetTrigger Trigger), PetState> _transitions;
    private CancellationTokenSource? _timerCts;

    public PetState CurrentState => _currentState;

    public event Action<PetState>? StateChanged;

    // Temporary state durations
    private static readonly Dictionary<PetState, TimeSpan> TemporaryDurations = new()
    {
        { PetState.Error, TimeSpan.FromSeconds(2) },
        { PetState.Success, TimeSpan.FromSeconds(3) },
    };

    public PetStateMachine()
    {
        _transitions = BuildTransitions();
    }

    private static Dictionary<(PetState, PetTrigger), PetState> BuildTransitions()
    {
        var transitions = new (PetState From, PetTrigger Trigger, PetState To)[]
        {
            // Idle
            (PetState.Idle, PetTrigger.SessionStart, PetState.Idle),
            (PetState.Idle, PetTrigger.PromptSubmit, PetState.Thinking),
            (PetState.Idle, PetTrigger.SessionEnd, PetState.Idle),

            // Thinking
            (PetState.Thinking, PetTrigger.ToolStart, PetState.Working),
            (PetState.Thinking, PetTrigger.SubagentStart, PetState.Thinking),
            (PetState.Thinking, PetTrigger.Stop, PetState.Success),
            (PetState.Thinking, PetTrigger.StopFailure, PetState.Error),
            (PetState.Thinking, PetTrigger.SessionEnd, PetState.Idle),
            (PetState.Thinking, PetTrigger.PromptSubmit, PetState.Thinking),

            // Working
            (PetState.Working, PetTrigger.ToolEnd, PetState.Working),
            (PetState.Working, PetTrigger.ToolStart, PetState.Working),
            (PetState.Working, PetTrigger.ToolFailure, PetState.Error),
            (PetState.Working, PetTrigger.SubagentStart, PetState.Thinking),
            (PetState.Working, PetTrigger.SubagentStop, PetState.Working),
            (PetState.Working, PetTrigger.Stop, PetState.Success),
            (PetState.Working, PetTrigger.StopFailure, PetState.Error),
            (PetState.Working, PetTrigger.SessionEnd, PetState.Idle),
            (PetState.Working, PetTrigger.PromptSubmit, PetState.Thinking),

            // Error (temporary - returns to previous state)
            (PetState.Error, PetTrigger.ToolStart, PetState.Working),
            (PetState.Error, PetTrigger.PromptSubmit, PetState.Thinking),
            (PetState.Error, PetTrigger.Stop, PetState.Success),
            (PetState.Error, PetTrigger.StopFailure, PetState.Error),
            (PetState.Error, PetTrigger.SessionEnd, PetState.Idle),

            // Success (temporary - returns to idle)
            (PetState.Success, PetTrigger.PromptSubmit, PetState.Thinking),
            (PetState.Success, PetTrigger.SessionEnd, PetState.Idle),
            (PetState.Success, PetTrigger.SessionStart, PetState.Idle),

            // Waiting (notification overlay)
            (PetState.Waiting, PetTrigger.NotificationIdle, PetState.Waiting),
            (PetState.Waiting, PetTrigger.NotificationPermission, PetState.Waiting),
            (PetState.Waiting, PetTrigger.PromptSubmit, PetState.Thinking),
            (PetState.Waiting, PetTrigger.ToolStart, PetState.Working),
            (PetState.Waiting, PetTrigger.SessionEnd, PetState.Idle),
            (PetState.Waiting, PetTrigger.Stop, PetState.Success),
        };

        var dict = new Dictionary<(PetState, PetTrigger), PetState>();
        foreach (var (from, trigger, to) in transitions)
        {
            dict[(from, trigger)] = to;
        }
        return dict;
    }

    public bool Transition(PetTrigger trigger)
    {
        // Handle notification triggers specially (overlay behavior)
        if (trigger is PetTrigger.NotificationIdle or PetTrigger.NotificationPermission)
        {
            if (_currentState != PetState.Waiting)
            {
                _stateBeforeNotification = _currentState;
            }
            SetState(PetState.Waiting);
            return true;
        }

        // Exit waiting state on any other trigger
        if (_currentState == PetState.Waiting)
        {
            _timerCts?.Cancel();
        }

        if (_transitions.TryGetValue((_currentState, trigger), out var newState))
        {
            SetState(newState);
            return true;
        }

        // Idempotent fallback: ignore unknown transitions
        return false;
    }

    private void SetState(PetState newState)
    {
        _timerCts?.Cancel();

        _currentState = newState;
        StateChanged?.Invoke(_currentState);

        // Handle temporary states
        if (TemporaryDurations.TryGetValue(newState, out var duration))
        {
            _timerCts = new CancellationTokenSource();
            _ = AutoRevertAsync(newState, duration, _timerCts.Token);
        }
    }

    private async Task AutoRevertAsync(PetState temporaryState, TimeSpan duration, CancellationToken ct)
    {
        try
        {
            await Task.Delay(duration, ct);

            if (_currentState == temporaryState)
            {
                var revertTo = temporaryState == PetState.Success ? PetState.Idle : _stateBeforeNotification;
                if (revertTo == PetState.Waiting) revertTo = PetState.Idle;
                _currentState = revertTo;
                StateChanged?.Invoke(_currentState);
            }
        }
        catch (OperationCanceledException)
        {
            // Timer was cancelled, state was already changed
        }
    }
}
