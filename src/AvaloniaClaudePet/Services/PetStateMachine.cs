using AvaloniaClaudePet.Models;

namespace AvaloniaClaudePet.Services;

public class PetStateMachine
{
    private readonly object _lock = new();
    private PetState _currentState = PetState.Idle;
    private PetState _stateBeforeTemporary = PetState.Idle;
    private readonly Dictionary<(PetState State, PetTrigger Trigger), PetState> _transitions;
    private CancellationTokenSource? _timerCts;

    public PetState CurrentState
    {
        get { lock (_lock) { return _currentState; } }
    }

    public event Action<PetState>? StateChanged;

    private static readonly Dictionary<PetState, TimeSpan> TemporaryDurations = new()
    {
        { PetState.Error, TimeSpan.FromSeconds(2) },
        { PetState.Success, TimeSpan.FromSeconds(3) },
    };

    // Auto-revert targets: what state to return to after a temporary state expires
    private static readonly Dictionary<PetState, PetState> TemporaryRevertTargets = new()
    {
        { PetState.Error, PetState.Idle },    // Error reverts to Idle per spec
        { PetState.Success, PetState.Idle },  // Success reverts to Idle per spec
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

            // Error (temporary - returns to working or idle)
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
        lock (_lock)
        {
            // Handle notification triggers specially (overlay behavior)
            if (trigger is PetTrigger.NotificationIdle or PetTrigger.NotificationPermission)
            {
                if (_currentState != PetState.Waiting)
                {
                    _stateBeforeTemporary = _currentState;
                }
                SetStateLocked(PetState.Waiting);
                return true;
            }

            if (_transitions.TryGetValue((_currentState, trigger), out var newState))
            {
                SetStateLocked(newState);
                return true;
            }

            return false;
        }
    }

    private void SetStateLocked(PetState newState)
    {
        _timerCts?.Cancel();
        _timerCts = null;

        _currentState = newState;
        StateChanged?.Invoke(_currentState);

        // Handle temporary states
        if (TemporaryDurations.TryGetValue(newState, out var duration))
        {
            _timerCts = new CancellationTokenSource();
            var ct = _timerCts.Token;
            var revertTo = TemporaryRevertTargets.GetValueOrDefault(newState, PetState.Idle);
            _ = AutoRevertAsync(newState, revertTo, duration, ct);
        }
    }

    private async Task AutoRevertAsync(PetState temporaryState, PetState revertTo, TimeSpan duration, CancellationToken ct)
    {
        try
        {
            await Task.Delay(duration, ct);

            lock (_lock)
            {
                if (_currentState == temporaryState)
                {
                    _currentState = revertTo;
                    StateChanged?.Invoke(_currentState);
                }
            }
        }
        catch (OperationCanceledException) { }
    }
}
