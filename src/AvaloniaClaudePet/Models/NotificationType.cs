namespace AvaloniaClaudePet.Models;

public enum NotificationType
{
    IdlePrompt,
    PermissionPrompt
}

public record NotificationInfo(NotificationType Type, string Message);
