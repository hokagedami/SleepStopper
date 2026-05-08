using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SleepStopper.Services;

namespace SleepStopper.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public const int MinIntervalSeconds = 5;
    public const int MaxIntervalSeconds = 600;

    [ObservableProperty]
    private bool _weekdaysOnly;

    [ObservableProperty]
    private int _intervalSeconds;

    [ObservableProperty]
    private bool _pauseWhenLocked;

    [ObservableProperty]
    private bool _autoStartSleepPrevention;

    [ObservableProperty]
    private bool _autoStartPresence;

    [ObservableProperty]
    private bool _window1Enabled;

    [ObservableProperty]
    private TimeSpan _window1Start;

    [ObservableProperty]
    private TimeSpan _window1End;

    [ObservableProperty]
    private bool _window2Enabled;

    [ObservableProperty]
    private TimeSpan _window2Start;

    [ObservableProperty]
    private TimeSpan _window2End;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    public event EventHandler<AppSettings>? Saved;
    public event EventHandler? Cancelled;

    public SettingsViewModel(AppSettings settings)
    {
        var p = settings.Presence;
        _weekdaysOnly = p.WeekdaysOnly;
        _intervalSeconds = ClampInterval(p.IntervalSeconds);
        _pauseWhenLocked = p.PauseWhenLocked;
        _autoStartSleepPrevention = settings.Startup.AutoStartSleepPrevention;
        _autoStartPresence = settings.Startup.AutoStartPresence;
        _window1Enabled = p.Window1.Enabled;
        _window1Start = ParseOrDefault(p.Window1.Start, new TimeSpan(9, 30, 0));
        _window1End = ParseOrDefault(p.Window1.End, new TimeSpan(12, 0, 0));
        _window2Enabled = p.Window2.Enabled;
        _window2Start = ParseOrDefault(p.Window2.Start, new TimeSpan(14, 0, 0));
        _window2End = ParseOrDefault(p.Window2.End, new TimeSpan(18, 0, 0));
    }

    private static int ClampInterval(int value)
    {
        if (value < MinIntervalSeconds) return MinIntervalSeconds;
        if (value > MaxIntervalSeconds) return MaxIntervalSeconds;
        return value;
    }

    private static TimeSpan ParseOrDefault(string text, TimeSpan fallback)
    {
        return TimeOnly.TryParse(text, out var t) ? t.ToTimeSpan() : fallback;
    }

    public AppSettings? Validate()
    {
        if (IntervalSeconds < MinIntervalSeconds || IntervalSeconds > MaxIntervalSeconds)
        {
            ValidationMessage = $"Interval must be between {MinIntervalSeconds} and {MaxIntervalSeconds} seconds.";
            return null;
        }
        if (Window1Enabled && Window1End <= Window1Start)
        {
            ValidationMessage = "Window 1 end must be after start.";
            return null;
        }
        if (Window2Enabled && Window2End <= Window2Start)
        {
            ValidationMessage = "Window 2 end must be after start.";
            return null;
        }
        if (!Window1Enabled && !Window2Enabled)
        {
            ValidationMessage = "At least one window must be enabled.";
            return null;
        }

        ValidationMessage = string.Empty;
        return new AppSettings
        {
            Presence = new PresenceSettings
            {
                WeekdaysOnly = WeekdaysOnly,
                IntervalSeconds = IntervalSeconds,
                PauseWhenLocked = PauseWhenLocked,
                Window1 = new PresenceWindowSettings
                {
                    Enabled = Window1Enabled,
                    Start = Format(Window1Start),
                    End = Format(Window1End)
                },
                Window2 = new PresenceWindowSettings
                {
                    Enabled = Window2Enabled,
                    Start = Format(Window2Start),
                    End = Format(Window2End)
                }
            },
            Startup = new StartupSettings
            {
                AutoStartSleepPrevention = AutoStartSleepPrevention,
                AutoStartPresence = AutoStartPresence
            }
        };
    }

    [RelayCommand]
    private void Save()
    {
        var settings = Validate();
        if (settings != null)
        {
            Saved?.Invoke(this, settings);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

    private static string Format(TimeSpan t) => $"{t.Hours:D2}:{t.Minutes:D2}";
}
