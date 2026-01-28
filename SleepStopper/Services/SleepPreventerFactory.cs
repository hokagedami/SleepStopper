using System.Runtime.InteropServices;

namespace SleepStopper.Services;

public static class SleepPreventerFactory
{
    public static ISleepPreventer Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsSleepPreventer();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new LinuxSleepPreventer();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new MacOSSleepPreventer();
        }
        else
        {
            throw new PlatformNotSupportedException("Sleep prevention is not supported on this platform.");
        }
    }
}
