using SleepStopper.Services;
using Xunit;

namespace SleepStopper.Tests.Unit;

public class BuildScheduleTests
{
    [Fact]
    public void BuildSchedule_IncludesBothEnabledWindows()
    {
        var settings = new PresenceSettings();
        var schedule = SettingsService.BuildSchedule(settings);
        Assert.Equal(2, schedule.Windows.Count);
    }

    [Fact]
    public void BuildSchedule_SkipsDisabledWindow()
    {
        var settings = new PresenceSettings();
        settings.Window2.Enabled = false;
        var schedule = SettingsService.BuildSchedule(settings);
        Assert.Single(schedule.Windows);
        Assert.Equal(new TimeOnly(9, 30), schedule.Windows[0].Start);
    }

    [Fact]
    public void BuildSchedule_SkipsWindowWithEndBeforeStart()
    {
        var settings = new PresenceSettings();
        settings.Window1.Start = "12:00";
        settings.Window1.End = "09:00";
        var schedule = SettingsService.BuildSchedule(settings);
        Assert.Single(schedule.Windows);
    }

    [Fact]
    public void BuildSchedule_SkipsWindowWithUnparseableTime()
    {
        var settings = new PresenceSettings();
        settings.Window1.Start = "not-a-time";
        var schedule = SettingsService.BuildSchedule(settings);
        Assert.Single(schedule.Windows);
    }

    [Fact]
    public void BuildSchedule_PropagatesWeekdaysOnly()
    {
        var settings = new PresenceSettings { WeekdaysOnly = false };
        var schedule = SettingsService.BuildSchedule(settings);
        Assert.False(schedule.WeekdaysOnly);
    }

    [Fact]
    public void BuildSchedule_EmptyWhenAllDisabled()
    {
        var settings = new PresenceSettings();
        settings.Window1.Enabled = false;
        settings.Window2.Enabled = false;
        var schedule = SettingsService.BuildSchedule(settings);
        Assert.Empty(schedule.Windows);
    }
}
