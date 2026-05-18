using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using AvaloniaClaudePet.Models;
using AvaloniaClaudePet.Services;
using AvaloniaClaudePet.ViewModels;
using AvaloniaClaudePet.Views;

namespace AvaloniaClaudePet;

public class App : Application
{
    private PetStateMachine? _stateMachine;
    private HookHttpServer? _httpServer;
    private NotificationService? _notificationService;
    private PetWindow? _petWindow;
    private BubbleWindow? _bubbleWindow;
    private HookConfigService? _hookConfigService;
    private TrayIcon? _trayIcon;

    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Core services
            _stateMachine = new PetStateMachine();
            _notificationService = new NotificationService(OnNotification);

            var petViewModel = new PetViewModel(_stateMachine);
            var bubbleViewModel = new BubbleViewModel();

            // Create windows
            _petWindow = new PetWindow { DataContext = petViewModel };
            _bubbleWindow = new BubbleWindow { DataContext = bubbleViewModel };

            // Start HTTP server
            _httpServer = new HookHttpServer(_stateMachine, _notificationService.Show);
            await _httpServer.StartAsync();

            // Hook config service
            _hookConfigService = new HookConfigService(_httpServer.Port);

            // System tray
            SetupTrayIcon();

            // Window events
            _petWindow.Closed += OnPetWindowClosed;

            desktop.MainWindow = _petWindow;

            // Show bubble window (hidden initially)
            _bubbleWindow.Show();
            _bubbleWindow.Hide();

            // Auto-configure hooks if not configured
            if (!_hookConfigService.IsConfigured())
            {
                _hookConfigService.ConfigureHooks();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnNotification(NotificationInfo? notification)
    {
        if (_bubbleWindow == null) return;

        if (notification != null)
        {
            var vm = (BubbleViewModel)_bubbleWindow.DataContext!;
            vm.ShowNotification(notification);

            // Position bubble above pet window
            if (_petWindow != null)
            {
                _bubbleWindow.Position = new PixelPoint(
                    _petWindow.Position.X - 40,
                    _petWindow.Position.Y - 70
                );
            }
            _bubbleWindow.Show();
        }
        else
        {
            ((BubbleViewModel)_bubbleWindow.DataContext!).Hide();
            _bubbleWindow.Hide();
        }
    }

    private void SetupTrayIcon()
    {
        var showPet = new NativeMenuItem("Show Pet");
        showPet.Click += (_, _) => _petWindow?.Show();

        var configureHooks = new NativeMenuItem("Configure Hooks");
        configureHooks.Click += (_, _) => _hookConfigService?.ConfigureHooks();

        var portInfo = new NativeMenuItem($"Port: {_httpServer?.Port ?? 0}");
        portInfo.Click += async (_, _) =>
        {
            if (_petWindow != null)
            {
                var clipboard = _petWindow.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync($"http://localhost:{_httpServer?.Port}");
                }
            }
        };

        var quit = new NativeMenuItem("Quit");
        quit.Click += (_, _) =>
        {
            _httpServer?.StopAsync().Wait();
            (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
        };

        var menu = new NativeMenu();
        menu.Items.Add(showPet);
        menu.Items.Add(configureHooks);
        menu.Items.Add(portInfo);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(quit);

        _trayIcon = new TrayIcon
        {
            Menu = menu,
            ToolTipText = "Claude Pet",
            IsVisible = true
        };
    }

    private void OnPetWindowClosed(object? sender, EventArgs e)
    {
        // Don't shutdown - minimize to tray
        // The window closing just hides it
    }
}
