using AvaloniaClaudePet.Models;
using AvaloniaClaudePet.Services;
using Xunit;

namespace AvaloniaClaudePet.Tests;

public class PetStateMachineTests
{
    [Fact]
    public void InitialState_IsIdle()
    {
        var sm = new PetStateMachine();
        Assert.Equal(PetState.Idle, sm.CurrentState);
    }

    [Fact]
    public void PromptSubmit_FromIdle_GoesToThinking()
    {
        var sm = new PetStateMachine();
        sm.Transition(PetTrigger.PromptSubmit);
        Assert.Equal(PetState.Thinking, sm.CurrentState);
    }

    [Fact]
    public void ToolStart_FromThinking_GoesToWorking()
    {
        var sm = new PetStateMachine();
        sm.Transition(PetTrigger.PromptSubmit);
        sm.Transition(PetTrigger.ToolStart);
        Assert.Equal(PetState.Working, sm.CurrentState);
    }

    [Fact]
    public void ToolFailure_FromWorking_GoesToError()
    {
        var sm = new PetStateMachine();
        sm.Transition(PetTrigger.PromptSubmit);
        sm.Transition(PetTrigger.ToolStart);
        sm.Transition(PetTrigger.ToolFailure);
        Assert.Equal(PetState.Error, sm.CurrentState);
    }

    [Fact]
    public void Stop_FromWorking_GoesToWaiting()
    {
        var sm = new PetStateMachine();
        sm.Transition(PetTrigger.PromptSubmit);
        sm.Transition(PetTrigger.ToolStart);
        sm.Transition(PetTrigger.Stop);
        Assert.Equal(PetState.Waiting, sm.CurrentState);
    }

    [Fact]
    public void StopFailure_FromWorking_GoesToError()
    {
        var sm = new PetStateMachine();
        sm.Transition(PetTrigger.PromptSubmit);
        sm.Transition(PetTrigger.ToolStart);
        sm.Transition(PetTrigger.StopFailure);
        Assert.Equal(PetState.Error, sm.CurrentState);
    }

    [Fact]
    public void SessionEnd_FromAnyState_GoesToIdle()
    {
        var sm = new PetStateMachine();
        sm.Transition(PetTrigger.PromptSubmit);
        sm.Transition(PetTrigger.ToolStart);
        sm.Transition(PetTrigger.SessionEnd);
        Assert.Equal(PetState.Idle, sm.CurrentState);
    }

    [Fact]
    public void NotificationIdle_OverlaysWaitingState()
    {
        var sm = new PetStateMachine();
        sm.Transition(PetTrigger.PromptSubmit);
        sm.Transition(PetTrigger.NotificationIdle);
        Assert.Equal(PetState.Waiting, sm.CurrentState);
    }

    [Fact]
    public void PromptSubmit_FromWaiting_ExitsWaiting()
    {
        var sm = new PetStateMachine();
        sm.Transition(PetTrigger.PromptSubmit);
        sm.Transition(PetTrigger.NotificationIdle);
        sm.Transition(PetTrigger.PromptSubmit);
        Assert.Equal(PetState.Thinking, sm.CurrentState);
    }

    [Fact]
    public void SubagentStart_FromThinking_StaysThinking()
    {
        var sm = new PetStateMachine();
        sm.Transition(PetTrigger.PromptSubmit);
        sm.Transition(PetTrigger.SubagentStart);
        Assert.Equal(PetState.Thinking, sm.CurrentState);
    }

    [Fact]
    public void MultipleToolUseCycles_StayInWorking()
    {
        var sm = new PetStateMachine();
        sm.Transition(PetTrigger.PromptSubmit);
        sm.Transition(PetTrigger.ToolStart);
        sm.Transition(PetTrigger.ToolEnd);
        Assert.Equal(PetState.Working, sm.CurrentState);
        sm.Transition(PetTrigger.ToolStart);
        sm.Transition(PetTrigger.ToolEnd);
        Assert.Equal(PetState.Working, sm.CurrentState);
    }

    [Fact]
    public void ToolStart_FromError_GoesToWorking()
    {
        var sm = new PetStateMachine();
        sm.Transition(PetTrigger.PromptSubmit);
        sm.Transition(PetTrigger.ToolStart);
        sm.Transition(PetTrigger.ToolFailure);
        Assert.Equal(PetState.Error, sm.CurrentState);
        sm.Transition(PetTrigger.ToolStart);
        Assert.Equal(PetState.Working, sm.CurrentState);
    }

    [Fact]
    public void StateChanged_FiresOnTransition()
    {
        var sm = new PetStateMachine();
        PetState? changedTo = null;
        sm.StateChanged += state => changedTo = state;
        sm.Transition(PetTrigger.PromptSubmit);
        Assert.Equal(PetState.Thinking, changedTo);
    }

    [Fact]
    public void SessionStart_FromIdle_StaysIdle()
    {
        var sm = new PetStateMachine();
        sm.Transition(PetTrigger.SessionStart);
        Assert.Equal(PetState.Idle, sm.CurrentState);
    }
}
