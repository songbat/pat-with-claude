using System.ComponentModel;
using Avalonia.Threading;
using AvaloniaClaudePet.Models;
using AvaloniaClaudePet.Services;

namespace AvaloniaClaudePet.ViewModels;

public class PetViewModel : INotifyPropertyChanged
{
    private readonly PetStateMachine _stateMachine;
    private readonly LocalizationService _localization;
    private PetState _currentState = PetState.Idle;
    private string _currentExpression = "idle";
    private string _statusText = "";
    private string _toolName = "";

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

    public string StatusText
    {
        get => _statusText;
        private set
        {
            if (_statusText != value)
            {
                _statusText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
            }
        }
    }

    public bool ShowBubble => !string.IsNullOrEmpty(_statusText);

    public event PropertyChangedEventHandler? PropertyChanged;

    public PetViewModel(PetStateMachine stateMachine, LocalizationService localization)
    {
        _stateMachine = stateMachine;
        _localization = localization;
        _stateMachine.StateChanged += OnStateChanged;
        _localization.LanguageChanged += OnLanguageChanged;
    }

    public void UpdateStatus(string statusKey, string? toolName = null)
    {
        _toolName = toolName ?? "";
        Dispatcher.UIThread.Post(() =>
        {
            StatusText = FormatStatus(statusKey, _toolName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowBubble)));
        });
    }

    public void ClearStatus()
    {
        Dispatcher.UIThread.Post(() =>
        {
            StatusText = "";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowBubble)));
        });
    }

    private string FormatStatus(string key, string tool)
    {
        var text = _localization[key];
        if (string.IsNullOrEmpty(text)) return "";
        if (!string.IsNullOrEmpty(tool)) return $"{text} {tool}";
        return text;
    }

    private void OnStateChanged(PetState newState)
    {
        Dispatcher.UIThread.Post(() =>
        {
            CurrentState = newState;
            CurrentExpression = StateToExpression(newState);

            var statusKey = newState switch
            {
                PetState.Thinking => "analyzing",
                PetState.Working => "processing",
                PetState.Success => "done",
                PetState.Error => "failed",
                PetState.Waiting => "waiting_answer",
                _ => ""
            };

            if (!string.IsNullOrEmpty(statusKey))
            {
                StatusText = FormatStatus(statusKey, _toolName);
            }
            else
            {
                StatusText = "";
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowBubble)));
        });
    }

    private void OnLanguageChanged()
    {
        // Re-render with new language
        OnStateChanged(_currentState);
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
