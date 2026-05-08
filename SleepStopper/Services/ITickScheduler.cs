using Avalonia.Threading;

namespace SleepStopper.Services;

public interface ITickScheduler : IDisposable
{
    event EventHandler? Tick;
    void Start();
    void Stop();
}

public sealed class DispatcherTickScheduler : ITickScheduler
{
    private readonly DispatcherTimer _timer;

    public event EventHandler? Tick;

    public DispatcherTickScheduler(TimeSpan interval)
    {
        _timer = new DispatcherTimer { Interval = interval };
        _timer.Tick += (s, e) => Tick?.Invoke(this, EventArgs.Empty);
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();
    public void Dispose() => _timer.Stop();
}
