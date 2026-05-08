using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SleepStopper.Services;

namespace SleepStopper.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly ISleepPreventer _sleepPreventer;
    private readonly IPresenceKeeper _presenceKeeper;
    private readonly SettingsService _settingsService;
    private readonly Func<TimeSpan, ITickScheduler>? _tickSchedulerFactory;
    private readonly Func<DateTime>? _nowProvider;
    private AppSettings _settings;
    private PresenceSchedule _schedule;
    private readonly StringBuilder _logBuilder;
    private ITickScheduler? _tickScheduler;
    private bool _wasNudgingLastTick;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private string _buttonText = "ACTIVATE";

    [ObservableProperty]
    private string _sleepStateLabel = "Inactive";

    [ObservableProperty]
    private bool _isPresenceActive;

    [ObservableProperty]
    private string _presenceButtonText = "START PRESENCE";

    [ObservableProperty]
    private string _presenceStatus = string.Empty;

    [ObservableProperty]
    private string _presenceStateLabel = "Idle";

    [ObservableProperty]
    private string _scheduleSummary = string.Empty;

    [ObservableProperty]
    private string _logText = string.Empty;

    [ObservableProperty]
    private string _currentSection = "dashboard";

    [ObservableProperty]
    private SettingsViewModel? _settingsPanel;

    public bool IsPresenceSupported => _presenceKeeper.IsSupported;
    public bool IsDashboardSelected => CurrentSection == "dashboard";
    public bool IsActivitySelected => CurrentSection == "activity";
    public bool IsSettingsSelected => CurrentSection == "settings";

    public event EventHandler? OpenSettingsRequested;

    public MainViewModel()
        : this(SleepPreventerFactory.Create(), PresenceKeeperFactory.Create(), new SettingsService(), null, null)
    {
    }

    public MainViewModel(
        ISleepPreventer sleepPreventer,
        IPresenceKeeper presenceKeeper,
        SettingsService settingsService,
        Func<TimeSpan, ITickScheduler>? tickSchedulerFactory = null,
        Func<DateTime>? nowProvider = null)
    {
        _sleepPreventer = sleepPreventer;
        _presenceKeeper = presenceKeeper;
        _settingsService = settingsService;
        _tickSchedulerFactory = tickSchedulerFactory;
        _nowProvider = nowProvider;
        _settings = _settingsService.Load();
        _schedule = SettingsService.BuildSchedule(_settings.Presence);
        _logBuilder = new StringBuilder();

        UpdateScheduleSummary();
        PresenceStatus = _presenceKeeper.IsSupported
            ? $"Within {ScheduleSummary}"
            : "Not supported on this platform";
        PresenceStateLabel = _presenceKeeper.IsSupported ? "Idle" : "Unavailable";

        AppendLog("Application started successfully....");
        AppendLog("System Auto-Sleep Active.");
    }

    public void RunStartupActions()
    {
        if (_settings.Startup.AutoStartSleepPrevention && !IsActive)
        {
            Toggle();
        }
        if (_settings.Startup.AutoStartPresence && !IsPresenceActive && _presenceKeeper.IsSupported)
        {
            TogglePresence();
        }
    }

    [RelayCommand]
    private void Toggle()
    {
        if (IsActive)
        {
            _sleepPreventer.Disable();
            IsActive = false;
            ButtonText = "ACTIVATE";
            SleepStateLabel = "Inactive";
            AppendLog("System Auto-Sleep Activated.");
        }
        else
        {
            _sleepPreventer.Enable();
            IsActive = true;
            ButtonText = "DEACTIVATE";
            SleepStateLabel = "Active";
            AppendLog("System Auto-Sleep Deactivated.");
        }
    }

    [RelayCommand]
    private void TogglePresence()
    {
        if (!_presenceKeeper.IsSupported)
        {
            AppendLog("Presence keeper is not supported on this platform.");
            return;
        }

        if (IsPresenceActive)
        {
            StopPresence("Presence stopped by user.");
        }
        else
        {
            StartPresence();
        }
    }

    [RelayCommand]
    private void OpenSettings()
    {
        SettingsPanel = new SettingsViewModel(_settings);
        SettingsPanel.Saved += OnSettingsSaved;
        SettingsPanel.Cancelled += OnSettingsCancelled;
        CurrentSection = "settings";
        OpenSettingsRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void NavigateDashboard() => CurrentSection = "dashboard";

    [RelayCommand]
    private void NavigateActivity() => CurrentSection = "activity";

    [RelayCommand]
    private void NavigateSettings() => OpenSettings();

    private void OnSettingsSaved(object? sender, AppSettings settings)
    {
        ApplySettings(settings);
        UnhookSettingsPanel();
        SettingsPanel = null;
        CurrentSection = "dashboard";
    }

    private void OnSettingsCancelled(object? sender, EventArgs e)
    {
        UnhookSettingsPanel();
        SettingsPanel = null;
        CurrentSection = "dashboard";
    }

    private void UnhookSettingsPanel()
    {
        if (SettingsPanel != null)
        {
            SettingsPanel.Saved -= OnSettingsSaved;
            SettingsPanel.Cancelled -= OnSettingsCancelled;
        }
    }

    partial void OnCurrentSectionChanged(string value)
    {
        OnPropertyChanged(nameof(IsDashboardSelected));
        OnPropertyChanged(nameof(IsActivitySelected));
        OnPropertyChanged(nameof(IsSettingsSelected));
    }

    public AppSettings CurrentSettings => _settings;

    public void ApplySettings(AppSettings settings)
    {
        _settings = settings;
        _settingsService.Save(settings);
        _schedule = SettingsService.BuildSchedule(settings.Presence);
        UpdateScheduleSummary();
        AppendLog($"Presence schedule updated: {ScheduleSummary}.");

        if (IsPresenceActive)
        {
            StopTickScheduler();
            StartTickScheduler();
            PresenceTick();
        }
        else
        {
            PresenceStatus = _presenceKeeper.IsSupported
                ? $"Within {ScheduleSummary}"
                : "Not supported on this platform";
        }
    }

    private void UpdateScheduleSummary()
    {
        if (_schedule.Windows.Count == 0)
        {
            ScheduleSummary = "no windows configured";
            return;
        }

        var prefix = _schedule.WeekdaysOnly ? "weekdays " : "daily ";
        ScheduleSummary = prefix + string.Join(", ",
            _schedule.Windows.Select(w => $"{w.Start:HH\\:mm}-{w.End:HH\\:mm}"));
    }

    private void StartPresence()
    {
        IsPresenceActive = true;
        PresenceButtonText = "STOP PRESENCE";
        PresenceStateLabel = "Active";
        _wasNudgingLastTick = false;
        AppendLog($"Active Presence started ({ScheduleSummary}).");
        StartTickScheduler();
        PresenceTick();
    }

    private void StartTickScheduler()
    {
        var interval = TimeSpan.FromSeconds(Math.Max(SettingsViewModel.MinIntervalSeconds, _settings.Presence.IntervalSeconds));
        _tickScheduler = _tickSchedulerFactory != null
            ? _tickSchedulerFactory(interval)
            : new DispatcherTickScheduler(interval);
        _tickScheduler.Tick += OnTickSchedulerTick;
        _tickScheduler.Start();
    }

    private void StopTickScheduler()
    {
        if (_tickScheduler != null)
        {
            _tickScheduler.Tick -= OnTickSchedulerTick;
            _tickScheduler.Stop();
            _tickScheduler.Dispose();
            _tickScheduler = null;
        }
    }

    private void OnTickSchedulerTick(object? sender, EventArgs e) => PresenceTick();

    private void StopPresence(string reason)
    {
        StopTickScheduler();
        IsPresenceActive = false;
        PresenceButtonText = "START PRESENCE";
        PresenceStateLabel = "Idle";
        _wasNudgingLastTick = false;
        PresenceStatus = $"Within {ScheduleSummary}";
        AppendLog(reason);
    }

    public void PresenceTick()
    {
        var now = _nowProvider != null ? _nowProvider() : DateTime.Now;
        var inWindow = _schedule.IsWorkTime(now);
        var locked = _settings.Presence.PauseWhenLocked && _presenceKeeper.IsScreenLocked();
        var shouldNudge = inWindow && !locked;

        if (shouldNudge)
        {
            try
            {
                _presenceKeeper.Nudge();
                if (!_wasNudgingLastTick)
                {
                    AppendLog($"Presence: simulating activity at {now:HH:mm:ss}.");
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Presence error: {ex.Message}");
            }
        }
        else if (_wasNudgingLastTick)
        {
            var reason = locked ? "screen locked" : "outside work hours";
            AppendLog($"Presence: paused ({reason}) at {now:HH:mm:ss}.");
        }

        _wasNudgingLastTick = shouldNudge;
        UpdatePresenceStatus(now, locked, shouldNudge);
    }

    private void UpdatePresenceStatus(DateTime now, bool screenLocked, bool nudging)
    {
        if (!IsPresenceActive)
        {
            return;
        }

        if (nudging)
        {
            PresenceStatus = $"Active (last nudge {now:HH:mm:ss})";
            PresenceStateLabel = "Active";
        }
        else if (screenLocked)
        {
            PresenceStatus = "Paused (screen locked)";
            PresenceStateLabel = "Paused";
        }
        else
        {
            PresenceStatus = $"Paused (outside {ScheduleSummary})";
            PresenceStateLabel = "Paused";
        }
    }

    public void Shutdown()
    {
        if (IsPresenceActive)
        {
            StopPresence("Presence stopped on shutdown.");
        }
        if (IsActive)
        {
            _sleepPreventer.Disable();
            AppendLog("Closing application....");
        }
    }

    private void AppendLog(string message)
    {
        if (_logBuilder.Length > 0)
        {
            _logBuilder.AppendLine();
        }
        _logBuilder.Append(message);
        LogText = _logBuilder.ToString();
    }

    public void Dispose()
    {
        StopTickScheduler();
        _presenceKeeper.Dispose();
        _sleepPreventer.Dispose();
    }
}
