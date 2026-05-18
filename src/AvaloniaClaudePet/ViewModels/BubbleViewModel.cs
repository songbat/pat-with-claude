using System.ComponentModel;
using Avalonia.Media.Imaging;
using AvaloniaClaudePet.Models;

namespace AvaloniaClaudePet.ViewModels;

public class BubbleViewModel : INotifyPropertyChanged
{
    private bool _isVisible;
    private string _message = string.Empty;
    private string _icon = string.Empty;

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
            }
        }
    }

    public string Message
    {
        get => _message;
        set
        {
            if (_message != value)
            {
                _message = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
            }
        }
    }

    public string Icon
    {
        get => _icon;
        set
        {
            if (_icon != value)
            {
                _icon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void ShowNotification(NotificationInfo notification)
    {
        Message = notification.Message;
        Icon = notification.Type switch
        {
            NotificationType.IdlePrompt => "⏳",
            NotificationType.PermissionPrompt => "\U0001F510",
            _ => "\U0001F4AC"
        };
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }
}
