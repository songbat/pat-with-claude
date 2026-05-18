using Avalonia.Threading;
using AvaloniaClaudePet.Models;

namespace AvaloniaClaudePet.Services;

public class NotificationService : IDisposable
{
    private readonly Action<NotificationInfo?> _displayAction;
    private CancellationTokenSource? _dismissCts;

    public NotificationService(Action<NotificationInfo?> displayAction)
    {
        _displayAction = displayAction;
    }

    public void Show(NotificationInfo notification)
    {
        _dismissCts?.Cancel();

        Dispatcher.UIThread.Post(() => _displayAction(notification));

        // Auto-dismiss after 10 seconds
        _dismissCts = new CancellationTokenSource();
        _ = AutoDismissAsync(_dismissCts.Token);
    }

    public void Dismiss()
    {
        _dismissCts?.Cancel();
        Dispatcher.UIThread.Post(() => _displayAction(null));
    }

    private async Task AutoDismissAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            Dispatcher.UIThread.Post(() => _displayAction(null));
        }
        catch (OperationCanceledException) { }
    }

    public void Dispose()
    {
        _dismissCts?.Cancel();
        _dismissCts?.Dispose();
    }
}
