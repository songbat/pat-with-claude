using System.ComponentModel;
using Avalonia.Threading;
using AvaloniaClaudePet.Models;
using AvaloniaClaudePet.Services;

namespace AvaloniaClaudePet.ViewModels;

public class PetViewModel : INotifyPropertyChanged
{
    private readonly PetStateMachine _stateMachine;
    private PetState _currentState = PetState.Idle;
    private string _currentExpression = "idle";

    public PetState CurrentState
    {
        get => _currentState;
        private set
        {
            if (_currentState != value)
            {
                _currentState = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentState)));
            }
        }
    }

    public string CurrentExpression
    {
        get => _currentExpression;
        private set
        {
            if (_currentExpression != value)
            {
                _currentExpression = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentExpression)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public PetViewModel(PetStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _stateMachine.StateChanged += OnStateChanged;
    }

    private void OnStateChanged(PetState newState)
    {
        Dispatcher.UIThread.Post(() =>
        {
            CurrentState = newState;
            CurrentExpression = StateToExpression(newState);
        });
    }

    private static string StateToExpression(PetState state) => state switch
    {
        PetState.Idle => "idle",
        PetState.Thinking => "thinking",
        PetState.Working => "working",
        PetState.Error => "error",
        PetState.Success => "success",
        PetState.Waiting => "waiting",
        _ => "idle"
    };
}
