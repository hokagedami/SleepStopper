using System.Text;
using Avalonia.Threading;
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
    private bool _isPresenceActive;

    [ObservableProperty]
    private string _presenceButtonText = "START PRESENCE";

    [ObservableProperty]
    private string _presenceStatus = string.Empty;

    [ObservableProperty]
    private string _scheduleSummary = string.Empty;

    [ObservableProperty]
    private string _logText = string.Empty;

    public bool IsPresenceSupported => _presenceKeeper.IsSupported;

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
            ? $"Presence: idle ({ScheduleSummary})"
            : "Presence: not supported on this platform";

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
            AppendLog("System Auto-Sleep Activated.");
        }
        else
        {
            _sleepPreventer.Enable();
            IsActive = true;
            ButtonText = "DEACTIVATE";
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
        OpenSettingsRequested?.Invoke(this, EventArgs.Empty);
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
                ? $"Presence: idle ({ScheduleSummary})"
                : "Presence: not supported on this platform";
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
        _wasNudgingLastTick = false;
        PresenceStatus = $"Presence: idle ({ScheduleSummary})";
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
            PresenceStatus = $"Presence: active (last nudge {now:HH:mm:ss})";
        }
        else if (screenLocked)
        {
            PresenceStatus = "Presence: paused (screen locked)";
        }
        else
        {
            PresenceStatus = $"Presence: paused (outside {ScheduleSummary})";
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
