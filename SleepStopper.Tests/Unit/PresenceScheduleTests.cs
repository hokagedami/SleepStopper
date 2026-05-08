using SleepStopper.Services;
using Xunit;

namespace SleepStopper.Tests.Unit;

public class PresenceScheduleTests
{
    private static readonly DateTime MondayMorning = new(2026, 1, 5, 10, 0, 0); // Monday
    private static readonly DateTime MondayLunch = new(2026, 1, 5, 12, 30, 0);
    private static readonly DateTime MondayAfternoon = new(2026, 1, 5, 15, 0, 0);
    private static readonly DateTime MondayEvening = new(2026, 1, 5, 19, 0, 0);
    private static readonly DateTime SaturdayMorning = new(2026, 1, 10, 10, 0, 0);
    private static readonly DateTime SundayAfternoon = new(2026, 1, 11, 15, 0, 0);

    private static PresenceSchedule TwoWindowSchedule(bool weekdaysOnly = true) => new(
        new[]
        {
            new PresenceWindow(new TimeOnly(9, 30), new TimeOnly(12, 0)),
            new PresenceWindow(new TimeOnly(14, 0), new TimeOnly(18, 0))
        },
        weekdaysOnly);

    [Fact]
    public void IsWorkTime_TrueInsideMorningWindowOnWeekday()
    {
        Assert.True(TwoWindowSchedule().IsWorkTime(MondayMorning));
    }

    [Fact]
    public void IsWorkTime_TrueInsideAfternoonWindowOnWeekday()
    {
        Assert.True(TwoWindowSchedule().IsWorkTime(MondayAfternoon));
    }

    [Fact]
    public void IsWorkTime_FalseDuringLunchGap()
    {
        Assert.False(TwoWindowSchedule().IsWorkTime(MondayLunch));
    }

    [Fact]
    public void IsWorkTime_FalseAfterAllWindows()
    {
        Assert.False(TwoWindowSchedule().IsWorkTime(MondayEvening));
    }

    [Fact]
    public void IsWorkTime_FalseOnSaturdayWhenWeekdaysOnly()
    {
        Assert.False(TwoWindowSchedule().IsWorkTime(SaturdayMorning));
    }

    [Fact]
    public void IsWorkTime_FalseOnSundayWhenWeekdaysOnly()
    {
        Assert.False(TwoWindowSchedule().IsWorkTime(SundayAfternoon));
    }

    [Fact]
    public void IsWorkTime_TrueOnWeekendWhenAllDays()
    {
        Assert.True(TwoWindowSchedule(weekdaysOnly: false).IsWorkTime(SaturdayMorning));
    }

    [Fact]
    public void IsWorkTime_FalseWhenNoWindows()
    {
        var schedule = new PresenceSchedule(Array.Empty<PresenceWindow>());
        Assert.False(schedule.IsWorkTime(MondayMorning));
    }
}
