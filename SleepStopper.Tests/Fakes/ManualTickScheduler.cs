using SleepStopper.Services;

namespace SleepStopper.Tests.Fakes;

public class ManualTickScheduler : ITickScheduler
{
    public TimeSpan Interval { get; }
    public bool IsRunning { get; private set; }

    public event EventHandler? Tick;

    public ManualTickScheduler(TimeSpan interval)
    {
        Interval = interval;
    }

    public void Start() => IsRunning = true;
    public void Stop() => IsRunning = false;

    public void RaiseTick()
    {
        Tick?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        IsRunning = false;
    }
}
