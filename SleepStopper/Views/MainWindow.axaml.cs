using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using SleepStopper.ViewModels;

namespace SleepStopper.Views;

public partial class MainWindow : Window
{
    private TrayIcon? _trayIcon;
    private bool _isExiting;

    public MainWindow()
    {
        InitializeComponent();
        SetupTrayIcon();
    }

    private void SetupTrayIcon()
    {
        var toggleMenuItem = new NativeMenuItem("ACTIVATE");
        toggleMenuItem.Click += (_, _) =>
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ToggleCommand.Execute(null);
                toggleMenuItem.Header = vm.IsActive ? "DEACTIVATE" : "ACTIVATE";
            }
        };

        var showMenuItem = new NativeMenuItem("Show");
        showMenuItem.Click += (_, _) => RestoreFromTray();

        var exitMenuItem = new NativeMenuItem("Exit");
        exitMenuItem.Click += (_, _) =>
        {
            _isExiting = true;
            if (DataContext is MainViewModel vm)
            {
                vm.Shutdown();
            }
            Close();
        };

        var menu = new NativeMenu();
        menu.Items.Add(toggleMenuItem);
        menu.Items.Add(showMenuItem);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(exitMenuItem);

        var iconUri = new Uri("avares://SleepStopper/sleep.ico");
        var iconStream = AssetLoader.Open(iconUri);
        var windowIcon = new WindowIcon(iconStream);

        _trayIcon = new TrayIcon
        {
            Icon = windowIcon,
            ToolTipText = "No Sleeping",
            Menu = menu,
            IsVisible = false
        };

        _trayIcon.Clicked += (_, _) => RestoreFromTray();

        // Update toggle menu item when state changes
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.IsActive))
                {
                    toggleMenuItem.Header = viewModel.IsActive ? "DEACTIVATE" : "ACTIVATE";
                }
            };
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MainViewModel viewModel && _trayIcon?.Menu?.Items.Count > 0)
        {
            var toggleMenuItem = _trayIcon.Menu.Items[0] as NativeMenuItem;
            if (toggleMenuItem != null)
            {
                viewModel.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(MainViewModel.IsActive))
                    {
                        toggleMenuItem.Header = viewModel.IsActive ? "DEACTIVATE" : "ACTIVATE";
                    }
                };
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WindowStateProperty
            && change.NewValue is WindowState state
            && state == WindowState.Minimized)
        {
            MinimizeToTray();
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_isExiting)
        {
            e.Cancel = true;
            MinimizeToTray();
        }
        else
        {
            _trayIcon?.Dispose();
            if (DataContext is MainViewModel vm)
            {
                vm.Dispose();
            }
        }
        base.OnClosing(e);
    }

    private void MinimizeToTray()
    {
        if (_trayIcon != null)
        {
            _trayIcon.IsVisible = true;
        }
        Hide();
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        if (_trayIcon != null)
        {
            _trayIcon.IsVisible = false;
        }
    }
}
