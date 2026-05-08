namespace SleepStopper.Services;

public record PresenceWindow(TimeOnly Start, TimeOnly End)
{
    public bool Contains(TimeOnly time) => time >= Start && time < End;
}

public class PresenceSchedule
{
    public IReadOnlyList<PresenceWindow> Windows { get; }
    public bool WeekdaysOnly { get; }

    public PresenceSchedule(IReadOnlyList<PresenceWindow> windows, bool weekdaysOnly = true)
    {
        Windows = windows;
        WeekdaysOnly = weekdaysOnly;
    }

    public bool IsWorkTime(DateTime now)
    {
        if (WeekdaysOnly && (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday))
        {
            return false;
        }

        var time = TimeOnly.FromDateTime(now);
        foreach (var window in Windows)
        {
            if (window.Contains(time))
            {
                return true;
            }
        }
        return false;
    }
}
