using SleepStopper.Services;
using SleepStopper.ViewModels;
using Xunit;

namespace SleepStopper.Tests.Unit;

public class SettingsViewModelTests
{
    [Fact]
    public void Constructor_HydratesAllFieldsFromSettings()
    {
        var settings = new AppSettings
        {
            Presence = new PresenceSettings
            {
                WeekdaysOnly = false,
                IntervalSeconds = 90,
                PauseWhenLocked = false,
                Window1 = new PresenceWindowSettings { Enabled = true, Start = "08:00", End = "11:00" },
                Window2 = new PresenceWindowSettings { Enabled = false, Start = "13:00", End = "16:00" }
            },
            Startup = new StartupSettings
            {
                AutoStartSleepPrevention = true,
                AutoStartPresence = true
            }
        };

        var vm = new SettingsViewModel(settings);

        Assert.False(vm.WeekdaysOnly);
        Assert.Equal(90, vm.IntervalSeconds);
        Assert.False(vm.PauseWhenLocked);
        Assert.True(vm.AutoStartSleepPrevention);
        Assert.True(vm.AutoStartPresence);
        Assert.True(vm.Window1Enabled);
        Assert.Equal(new TimeSpan(8, 0, 0), vm.Window1Start);
        Assert.Equal(new TimeSpan(11, 0, 0), vm.Window1End);
        Assert.False(vm.Window2Enabled);
    }

    [Fact]
    public void Constructor_ClampsTooSmallInterval()
    {
        var settings = new AppSettings { Presence = new PresenceSettings { IntervalSeconds = 1 } };
        var vm = new SettingsViewModel(settings);
        Assert.Equal(SettingsViewModel.MinIntervalSeconds, vm.IntervalSeconds);
    }

    [Fact]
    public void Constructor_ClampsTooLargeInterval()
    {
        var settings = new AppSettings { Presence = new PresenceSettings { IntervalSeconds = 99999 } };
        var vm = new SettingsViewModel(settings);
        Assert.Equal(SettingsViewModel.MaxIntervalSeconds, vm.IntervalSeconds);
    }

    [Fact]
    public void Validate_RejectsWhenWindow1EndBeforeStart()
    {
        var vm = new SettingsViewModel(new AppSettings());
        vm.Window1Start = new TimeSpan(12, 0, 0);
        vm.Window1End = new TimeSpan(10, 0, 0);

        var result = vm.Validate();

        Assert.Null(result);
        Assert.Contains("Window 1", vm.ValidationMessage);
    }

    [Fact]
    public void Validate_RejectsWhenBothWindowsDisabled()
    {
        var vm = new SettingsViewModel(new AppSettings());
        vm.Window1Enabled = false;
        vm.Window2Enabled = false;

        var result = vm.Validate();

        Assert.Null(result);
        Assert.Contains("At least one window", vm.ValidationMessage);
    }

    [Fact]
    public void Validate_AcceptsValidConfiguration()
    {
        var vm = new SettingsViewModel(new AppSettings());

        var result = vm.Validate();

        Assert.NotNull(result);
        Assert.Empty(vm.ValidationMessage);
    }

    [Fact]
    public void SaveCommand_RaisesSavedEventWhenValid()
    {
        var vm = new SettingsViewModel(new AppSettings());
        AppSettings? captured = null;
        vm.Saved += (_, s) => captured = s;

        vm.SaveCommand.Execute(null);

        Assert.NotNull(captured);
    }

    [Fact]
    public void SaveCommand_DoesNotRaiseSavedWhenInvalid()
    {
        var vm = new SettingsViewModel(new AppSettings());
        vm.Window1Enabled = false;
        vm.Window2Enabled = false;
        var raised = false;
        vm.Saved += (_, _) => raised = true;

        vm.SaveCommand.Execute(null);

        Assert.False(raised);
    }

    [Fact]
    public void CancelCommand_RaisesCancelledEvent()
    {
        var vm = new SettingsViewModel(new AppSettings());
        var raised = false;
        vm.Cancelled += (_, _) => raised = true;

        vm.CancelCommand.Execute(null);

        Assert.True(raised);
    }
}
