using Avalonia.Controls;
using Avalonia.Threading;

namespace AvaloniaClaudePet.Views;

public partial class BubbleWindow : Window
{
    public BubbleWindow()
    {
        InitializeComponent();
        Topmost = true;
    }

    public async Task ShowAnimated()
    {
        Opacity = 0;
        Show();
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Task.Delay(16);
            Opacity = 1;
        });
    }

    public async Task HideAnimated()
    {
        Opacity = 0;
        await Task.Delay(300);
        Hide();
        Opacity = 1;
    }
}
