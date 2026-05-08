using SleepStopper.Services;
using Xunit;

namespace SleepStopper.Tests.Unit;

public class PresenceWindowTests
{
    [Theory]
    [InlineData(9, 30, true)]
    [InlineData(10, 0, true)]
    [InlineData(11, 59, true)]
    [InlineData(9, 29, false)]
    [InlineData(12, 0, false)]
    [InlineData(12, 1, false)]
    [InlineData(0, 0, false)]
    public void Contains_RespectsHalfOpenInterval(int hour, int minute, bool expected)
    {
        var window = new PresenceWindow(new TimeOnly(9, 30), new TimeOnly(12, 0));
        Assert.Equal(expected, window.Contains(new TimeOnly(hour, minute)));
    }

    [Fact]
    public void Contains_StartTimeIsInclusive()
    {
        var window = new PresenceWindow(new TimeOnly(14, 0), new TimeOnly(18, 0));
        Assert.True(window.Contains(new TimeOnly(14, 0)));
    }

    [Fact]
    public void Contains_EndTimeIsExclusive()
    {
        var window = new PresenceWindow(new TimeOnly(14, 0), new TimeOnly(18, 0));
        Assert.False(window.Contains(new TimeOnly(18, 0)));
    }
}
