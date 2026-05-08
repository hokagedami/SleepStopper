namespace SleepStopper.Services;

public class PresenceWindowSettings
{
    public bool Enabled { get; set; } = true;
    public string Start { get; set; } = "09:30";
    public string End { get; set; } = "12:00";
}

public class PresenceSettings
{
    public bool WeekdaysOnly { get; set; } = true;
    public int IntervalSeconds { get; set; } = 55;
    public bool PauseWhenLocked { get; set; } = true;
    public PresenceWindowSettings Window1 { get; set; } = new();
    public PresenceWindowSettings Window2 { get; set; } = new() { Start = "14:00", End = "18:00" };
}

public class StartupSettings
{
    public bool AutoStartSleepPrevention { get; set; } = false;
    public bool AutoStartPresence { get; set; } = false;
}

public class AppSettings
{
    public PresenceSettings Presence { get; set; } = new();
    public StartupSettings Startup { get; set; } = new();
}
