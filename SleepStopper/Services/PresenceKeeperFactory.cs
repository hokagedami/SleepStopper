using System.Runtime.InteropServices;

namespace SleepStopper.Services;

public static class PresenceKeeperFactory
{
    public static IPresenceKeeper Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsPresenceKeeper();
        }

        return new NoOpPresenceKeeper();
    }
}
