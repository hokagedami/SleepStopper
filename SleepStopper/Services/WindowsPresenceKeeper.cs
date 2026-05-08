using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SleepStopper.Services;

public partial class WindowsPresenceKeeper : IPresenceKeeper
{
    private const ushort VK_NUMLOCK = 0x90;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint INPUT_KEYBOARD = 1;

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nuint dwExtraInfo;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUT_UNION
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public INPUT_UNION u;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial uint SendInput(uint nInputs, [In] INPUT[] pInputs, int cbSize);

    public bool IsSupported => true;

    public bool IsScreenLocked()
    {
        return Process.GetProcessesByName("LogonUI").Length > 0;
    }

    public void Nudge()
    {
        var inputs = new INPUT[]
        {
            new() { type = INPUT_KEYBOARD, u = new INPUT_UNION { ki = new KEYBDINPUT { wVk = VK_NUMLOCK } } },
            new() { type = INPUT_KEYBOARD, u = new INPUT_UNION { ki = new KEYBDINPUT { wVk = VK_NUMLOCK, dwFlags = KEYEVENTF_KEYUP } } },
            new() { type = INPUT_KEYBOARD, u = new INPUT_UNION { ki = new KEYBDINPUT { wVk = VK_NUMLOCK } } },
            new() { type = INPUT_KEYBOARD, u = new INPUT_UNION { ki = new KEYBDINPUT { wVk = VK_NUMLOCK, dwFlags = KEYEVENTF_KEYUP } } }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    public void Dispose()
    {
    }
}
