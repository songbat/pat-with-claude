using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
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
    private LocalizationService? _localizationService;
    private PetViewModel? _petViewModel;
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
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Core services
            _stateMachine = new PetStateMachine();
            _notificationService = new NotificationService(OnNotification);
            _localizationService = new LocalizationService();

            _petViewModel = new PetViewModel(_stateMachine, _localizationService);
            var bubbleViewModel = new BubbleViewModel();

            _petWindow = new PetWindow { DataContext = _petViewModel };
            _bubbleWindow = new BubbleWindow { DataContext = bubbleViewModel };

            _httpServer = new HookHttpServer(
                _stateMachine,
                _notificationService.Show,
                _notificationService.Dismiss,
                OnStatusUpdate);
            await _httpServer.StartAsync();

            _hookConfigService = new HookConfigService(_httpServer.Port);

            SetupTrayIcon();

            _petWindow.Closing += (s, e) =>
            {
                e.Cancel = true;
                ((Window)s!).Hide();
            };

            desktop.MainWindow = _petWindow;

            if (!_hookConfigService.IsConfigured())
            {
                _hookConfigService.ConfigureHooks();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnStatusUpdate(string statusKey, string? toolName)
    {
        if (_petViewModel == null) return;
        if (string.IsNullOrEmpty(statusKey))
        {
            _petViewModel.ClearStatus();
        }
        else
        {
            _petViewModel.UpdateStatus(statusKey, toolName);
        }
    }

    private void OnNotification(NotificationInfo? notification)
    {
        if (_bubbleWindow == null) return;

        Dispatcher.UIThread.Post(() =>
        {
            if (notification != null)
            {
                var vm = (BubbleViewModel)_bubbleWindow.DataContext!;
                vm.ShowNotification(notification);

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
        });
    }

    private void SetupTrayIcon()
    {
        var showPet = new NativeMenuItem("Show Pet");
        showPet.Click += (_, _) =>
        {
            _petWindow?.Show();
            _petWindow?.Activate();
        };

        var configureHooks = new NativeMenuItem("Configure Hooks");
        configureHooks.Click += (_, _) => _hookConfigService?.ConfigureHooks();

        var toggleLang = new NativeMenuItem(_localizationService?.CurrentLabel ?? "中/EN");
        toggleLang.Click += (_, _) =>
        {
            if (_localizationService != null)
            {
                _localizationService.Toggle();
                toggleLang.Header = _localizationService.CurrentLabel;
            }
        };

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
        quit.Click += async (_, _) =>
        {
            if (_httpServer != null) await _httpServer.StopAsync();
            (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
        };

        var menu = new NativeMenu();
        menu.Items.Add(showPet);
        menu.Items.Add(configureHooks);
        menu.Items.Add(toggleLang);
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
}
