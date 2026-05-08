using SleepStopper.Services;
using SleepStopper.Tests.Fakes;
using SleepStopper.ViewModels;
using Xunit;

namespace SleepStopper.Tests.E2E;

public class UserJourneyTests
{
    private static readonly DateTime MondayMorning = new(2026, 1, 5, 10, 0, 0);
    private static readonly DateTime MondayLunch = new(2026, 1, 5, 12, 30, 0);
    private static readonly DateTime SundayMorning = new(2026, 1, 11, 10, 0, 0);

    private sealed class TestHarness : IDisposable
    {
        public TempSettingsService Temp { get; }
        public FakeSleepPreventer Sleep { get; } = new();
        public FakePresenceKeeper Presence { get; } = new();
        public ManualTickScheduler? Scheduler { get; private set; }
        public DateTime Now { get; set; }
        public MainViewModel Vm { get; }

        public TestHarness(DateTime now, AppSettings? seed = null)
        {
            Now = now;
            Temp = new TempSettingsService();
            if (seed != null) Temp.Service.Save(seed);
            Vm = new MainViewModel(
                Sleep, Presence, Temp.Service,
                interval => Scheduler = new ManualTickScheduler(interval),
                () => Now);
        }

        public void Dispose() => Temp.Dispose();
    }

    [Fact]
    public void Journey_NewUser_TogglesSleepPrevention_SeesLogUpdate()
    {
        using var h = new TestHarness(MondayMorning);

        Assert.False(h.Vm.IsActive);
        Assert.Contains("Application started", h.Vm.LogText);

        h.Vm.ToggleCommand.Execute(null);

        Assert.True(h.Vm.IsActive);
        Assert.Equal(1, h.Sleep.EnableCount);
        Assert.Contains("Sleep Deactivated", h.Vm.LogText);

        h.Vm.ToggleCommand.Execute(null);

        Assert.False(h.Vm.IsActive);
        Assert.Equal(1, h.Sleep.DisableCount);
    }

    [Fact]
    public void Journey_UserCustomizesSchedule_PersistsAcrossRestart()
    {
        using var h1 = new TestHarness(MondayMorning);

        var sv = new SettingsViewModel(h1.Vm.CurrentSettings)
        {
            IntervalSeconds = 30,
            Window1Start = new TimeSpan(8, 0, 0),
            Window1End = new TimeSpan(10, 30, 0),
            Window2Enabled = false
        };
        var saved = sv.Validate();
        Assert.NotNull(saved);
        h1.Vm.ApplySettings(saved!);
        h1.Vm.Dispose();

        var service2 = new SettingsService(h1.Temp.Path);
        using var vm2 = new MainViewModel(new FakeSleepPreventer(), new FakePresenceKeeper(), service2);

        Assert.Equal(30, vm2.CurrentSettings.Presence.IntervalSeconds);
        Assert.Equal("08:00", vm2.CurrentSettings.Presence.Window1.Start);
        Assert.False(vm2.CurrentSettings.Presence.Window2.Enabled);
        Assert.Contains("08:00-10:30", vm2.ScheduleSummary);
        Assert.DoesNotContain("14:00-18:00", vm2.ScheduleSummary);
    }

    [Fact]
    public void Journey_PresenceLifecycle_NudgesOnlyDuringWindow()
    {
        using var h = new TestHarness(MondayMorning);

        h.Vm.TogglePresenceCommand.Execute(null);

        Assert.True(h.Vm.IsPresenceActive);
        Assert.NotNull(h.Scheduler);
        Assert.True(h.Scheduler!.IsRunning);

        var initialNudges = h.Presence.NudgeCount;
        Assert.True(initialNudges > 0);

        // Tick during lunch break — should not nudge
        h.Now = MondayLunch;
        h.Scheduler.RaiseTick();
        var afterLunch = h.Presence.NudgeCount;
        Assert.Equal(initialNudges, afterLunch);
        Assert.Contains("outside", h.Vm.PresenceStatus);

        // Tick back inside afternoon window
        h.Now = new DateTime(2026, 1, 5, 15, 0, 0);
        h.Scheduler.RaiseTick();
        Assert.True(h.Presence.NudgeCount > afterLunch);

        h.Vm.TogglePresenceCommand.Execute(null);
        Assert.False(h.Vm.IsPresenceActive);
        Assert.False(h.Scheduler.IsRunning);
    }

    [Fact]
    public void Journey_WeekdaysOnly_DoesNotNudgeOnSunday()
    {
        using var h = new TestHarness(SundayMorning);

        h.Vm.TogglePresenceCommand.Execute(null);
        var before = h.Presence.NudgeCount;
        h.Scheduler!.RaiseTick();

        Assert.Equal(before, h.Presence.NudgeCount);
        Assert.Contains("outside", h.Vm.PresenceStatus);
    }

    [Fact]
    public void Journey_AutoStart_KicksOffBothFeaturesOnLaunch()
    {
        var seed = new AppSettings
        {
            Startup = new StartupSettings
            {
                AutoStartSleepPrevention = true,
                AutoStartPresence = true
            }
        };
        using var h = new TestHarness(MondayMorning, seed);

        h.Vm.RunStartupActions();

        Assert.True(h.Vm.IsActive);
        Assert.True(h.Vm.IsPresenceActive);
        Assert.Equal(1, h.Sleep.EnableCount);
        Assert.True(h.Presence.NudgeCount > 0);
    }

    [Fact]
    public void Journey_ScreenLockMidSession_PausesAndResumes()
    {
        using var h = new TestHarness(MondayMorning);

        h.Vm.TogglePresenceCommand.Execute(null);
        var initial = h.Presence.NudgeCount;
        Assert.True(initial > 0);

        h.Presence.ScreenLocked = true;
        h.Scheduler!.RaiseTick();
        var afterLock = h.Presence.NudgeCount;
        Assert.Equal(initial, afterLock);
        Assert.Contains("locked", h.Vm.PresenceStatus);

        h.Presence.ScreenLocked = false;
        h.Scheduler.RaiseTick();
        Assert.True(h.Presence.NudgeCount > afterLock);
    }

    [Fact]
    public void Journey_ApplySettingsWhilePresenceRunning_ReplacesSchedule()
    {
        using var h = new TestHarness(MondayMorning);

        h.Vm.TogglePresenceCommand.Execute(null);
        Assert.True(h.Scheduler!.IsRunning);
        var oldScheduler = h.Scheduler;

        var newSettings = new AppSettings
        {
            Presence = new PresenceSettings
            {
                IntervalSeconds = 30,
                Window1 = new PresenceWindowSettings { Enabled = true, Start = "00:00", End = "23:59" },
                Window2 = new PresenceWindowSettings { Enabled = false }
            }
        };
        h.Vm.ApplySettings(newSettings);

        Assert.NotSame(oldScheduler, h.Scheduler);
        Assert.True(h.Scheduler.IsRunning);
        Assert.Equal(TimeSpan.FromSeconds(30), h.Scheduler.Interval);
        Assert.False(oldScheduler.IsRunning);
    }
}
