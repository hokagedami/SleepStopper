using SleepStopper.Services;

namespace SleepStopper.Tests.Fakes;

public class FakeSleepPreventer : ISleepPreventer
{
    public bool IsActive { get; private set; }
    public int EnableCount { get; private set; }
    public int DisableCount { get; private set; }
    public int DisposeCount { get; private set; }

    public void Enable()
    {
        EnableCount++;
        IsActive = true;
    }

    public void Disable()
    {
        DisableCount++;
        IsActive = false;
    }

    public void Dispose()
    {
        DisposeCount++;
    }
}
