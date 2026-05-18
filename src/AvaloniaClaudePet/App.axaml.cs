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
    private NativeMenuItem? _showPetItem;
    private NativeMenuItem? _configureHooksItem;
    private NativeMenuItem? _portInfoItem;
    private NativeMenuItem? _quitItem;

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
        _showPetItem = new NativeMenuItem(_localizationService?["menu_show"]);
        _showPetItem.Click += (_, _) =>
        {
            _petWindow?.Show();
            _petWindow?.Activate();
        };

        _configureHooksItem = new NativeMenuItem(_localizationService?["menu_hooks"]);
        _configureHooksItem.Click += (_, _) => _hookConfigService?.ConfigureHooks();

        var toggleLang = new NativeMenuItem(_localizationService?.CurrentLabel ?? "中/EN");
        toggleLang.Click += (_, _) =>
        {
            if (_localizationService != null)
            {
                _localizationService.Toggle();
                toggleLang.Header = _localizationService.CurrentLabel;
            }
        };

        _portInfoItem = new NativeMenuItem($"Port: {_httpServer?.Port ?? 0}");
        _portInfoItem.Click += async (_, _) =>
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

        _quitItem = new NativeMenuItem(_localizationService?["menu_quit"]);
        _quitItem.Click += async (_, _) =>
        {
            if (_httpServer != null) await _httpServer.StopAsync();
            (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
        };

        var menu = new NativeMenu();
        menu.Items.Add(_showPetItem);
        menu.Items.Add(_configureHooksItem);
        menu.Items.Add(toggleLang);
        menu.Items.Add(_portInfoItem);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(_quitItem);

        _trayIcon = new TrayIcon
        {
            Menu = menu,
            ToolTipText = _localizationService?["menu_tooltip"] ?? "Claude Pet",
            IsVisible = true
        };

        if (_localizationService != null)
        {
            _localizationService.LanguageChanged += UpdateMenuLabels;
        }
    }

    private void UpdateMenuLabels()
    {
        if (_localizationService == null) return;
        if (_showPetItem != null) _showPetItem.Header = _localizationService["menu_show"];
        if (_configureHooksItem != null) _configureHooksItem.Header = _localizationService["menu_hooks"];
        if (_portInfoItem != null) _portInfoItem.Header = $"Port: {_httpServer?.Port ?? 0}";
        if (_quitItem != null) _quitItem.Header = _localizationService["menu_quit"];
        if (_trayIcon != null) _trayIcon.ToolTipText = _localizationService["menu_tooltip"];
    }
}
