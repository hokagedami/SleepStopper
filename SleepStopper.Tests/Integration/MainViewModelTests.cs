using SleepStopper.Services;
using SleepStopper.Tests.Fakes;
using SleepStopper.ViewModels;
using Xunit;

namespace SleepStopper.Tests.Integration;

public class MainViewModelTests
{
    private static (MainViewModel vm, FakeSleepPreventer sleep, FakePresenceKeeper presence, ManualTickScheduler scheduler, TempSettingsService temp)
        BuildVm(DateTime? now = null, AppSettings? seed = null)
    {
        var temp = new TempSettingsService();
        if (seed != null)
        {
            temp.Service.Save(seed);
        }
        var sleep = new FakeSleepPreventer();
        var presence = new FakePresenceKeeper();
        ManualTickScheduler? scheduler = null;
        var fixedNow = now ?? new DateTime(2026, 1, 5, 10, 0, 0); // Monday 10:00
        var vm = new MainViewModel(
            sleep,
            presence,
            temp.Service,
            interval => scheduler = new ManualTickScheduler(interval),
            () => fixedNow);
        return (vm, sleep, presence, scheduler!, temp);
    }

    [Fact]
    public void ToggleCommand_TogglesSleepPrevention()
    {
        var (vm, sleep, _, _, temp) = BuildVm();
        using (temp)
        {
            vm.ToggleCommand.Execute(null);
            Assert.True(vm.IsActive);
            Assert.Equal(1, sleep.EnableCount);
            Assert.Equal("DEACTIVATE", vm.ButtonText);

            vm.ToggleCommand.Execute(null);
            Assert.False(vm.IsActive);
            Assert.Equal(1, sleep.DisableCount);
            Assert.Equal("ACTIVATE", vm.ButtonText);
        }
    }

    [Fact]
    public void TogglePresenceCommand_StartsAndStopsScheduler()
    {
        var (vm, _, _, _, temp) = BuildVm();
        using (temp)
        {
            vm.TogglePresenceCommand.Execute(null);
            Assert.True(vm.IsPresenceActive);
            Assert.Equal("STOP PRESENCE", vm.PresenceButtonText);

            vm.TogglePresenceCommand.Execute(null);
            Assert.False(vm.IsPresenceActive);
            Assert.Equal("START PRESENCE", vm.PresenceButtonText);
        }
    }

    [Fact]
    public void TogglePresence_DoesNothingWhenNotSupported()
    {
        var temp = new TempSettingsService();
        var presence = new FakePresenceKeeper { IsSupported = false };
        var vm = new MainViewModel(new FakeSleepPreventer(), presence, temp.Service);
        using (temp)
        {
            vm.TogglePresenceCommand.Execute(null);
            Assert.False(vm.IsPresenceActive);
            Assert.Contains("not supported", vm.LogText);
        }
    }

    [Fact]
    public void PresenceTick_NudgesWhenInsideWindow()
    {
        var monday10 = new DateTime(2026, 1, 5, 10, 0, 0);
        var (vm, _, presence, _, temp) = BuildVm(monday10);
        using (temp)
        {
            vm.TogglePresenceCommand.Execute(null);
            var startCount = presence.NudgeCount;
            vm.PresenceTick();
            Assert.True(presence.NudgeCount > startCount);
        }
    }

    [Fact]
    public void PresenceTick_DoesNotNudgeOutsideWindow()
    {
        var monday20 = new DateTime(2026, 1, 5, 20, 0, 0);
        var (vm, _, presence, _, temp) = BuildVm(monday20);
        using (temp)
        {
            vm.TogglePresenceCommand.Execute(null);
            var before = presence.NudgeCount;
            vm.PresenceTick();
            Assert.Equal(before, presence.NudgeCount);
            Assert.Contains("outside", vm.PresenceStatus);
        }
    }

    [Fact]
    public void PresenceTick_DoesNotNudgeWhenScreenLocked()
    {
        var monday10 = new DateTime(2026, 1, 5, 10, 0, 0);
        var (vm, _, presence, _, temp) = BuildVm(monday10);
        using (temp)
        {
            presence.ScreenLocked = true;
            vm.TogglePresenceCommand.Execute(null);
            var before = presence.NudgeCount;
            vm.PresenceTick();
            Assert.Equal(before, presence.NudgeCount);
            Assert.Contains("locked", vm.PresenceStatus);
        }
    }

    [Fact]
    public void PresenceTick_NudgesWhenLockedButPauseWhenLockedDisabled()
    {
        var monday10 = new DateTime(2026, 1, 5, 10, 0, 0);
        var seed = new AppSettings { Presence = new PresenceSettings { PauseWhenLocked = false } };
        var (vm, _, presence, _, temp) = BuildVm(monday10, seed);
        using (temp)
        {
            presence.ScreenLocked = true;
            vm.TogglePresenceCommand.Execute(null);
            var before = presence.NudgeCount;
            vm.PresenceTick();
            Assert.True(presence.NudgeCount > before);
        }
    }

    [Fact]
    public void ApplySettings_PersistsToDiskAndRebuildsSchedule()
    {
        var (vm, _, _, _, temp) = BuildVm();
        using (temp)
        {
            var newSettings = new AppSettings
            {
                Presence = new PresenceSettings
                {
                    WeekdaysOnly = false,
                    IntervalSeconds = 120,
                    Window1 = new PresenceWindowSettings { Enabled = true, Start = "08:00", End = "10:00" },
                    Window2 = new PresenceWindowSettings { Enabled = false }
                }
            };

            vm.ApplySettings(newSettings);

            Assert.True(File.Exists(temp.Path));
            var reloaded = temp.Service.Load();
            Assert.False(reloaded.Presence.WeekdaysOnly);
            Assert.Equal(120, reloaded.Presence.IntervalSeconds);
            Assert.Contains("08:00-10:00", vm.ScheduleSummary);
        }
    }

    [Fact]
    public void RunStartupActions_AutoStartsSleepPrevention()
    {
        var seed = new AppSettings { Startup = new StartupSettings { AutoStartSleepPrevention = true } };
        var (vm, sleep, _, _, temp) = BuildVm(seed: seed);
        using (temp)
        {
            vm.RunStartupActions();
            Assert.True(vm.IsActive);
            Assert.Equal(1, sleep.EnableCount);
        }
    }

    [Fact]
    public void RunStartupActions_AutoStartsPresence()
    {
        var seed = new AppSettings { Startup = new StartupSettings { AutoStartPresence = true } };
        var (vm, _, _, _, temp) = BuildVm(seed: seed);
        using (temp)
        {
            vm.RunStartupActions();
            Assert.True(vm.IsPresenceActive);
        }
    }

    [Fact]
    public void Shutdown_StopsPresenceAndDisablesSleep()
    {
        var (vm, sleep, _, _, temp) = BuildVm();
        using (temp)
        {
            vm.ToggleCommand.Execute(null);
            vm.TogglePresenceCommand.Execute(null);

            vm.Shutdown();

            Assert.False(vm.IsPresenceActive);
            Assert.Equal(1, sleep.DisableCount);
        }
    }
}
