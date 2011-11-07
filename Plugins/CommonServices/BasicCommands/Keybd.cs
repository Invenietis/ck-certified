using System;

namespace CommonServices
{
    public enum VKeyCode : byte
    {
        VK_SHIFT = 0xA0,
        VK_CONTROL = 0xA2,
        VK_MENU = 0xA4,
        VK_CAPITAL = 0x14,
        VK_ALTGR = 0xA5,
        VK_WIN = 0x5B
    }

    public class Keybd
    {
        public const byte SC_ALTGR_FR = 0x38;
        public const byte UP = 2;
        public const byte CTRL = 17;
        public const byte ESC = 27;

        public enum KEYEVENTF : uint
        {
            KEYDOWN = 0x0,
            EXTENDEDKEY = 0x1,
            KEYUP = 0x2
        }

        public enum VKeyCode : byte
        {
            VK_SHIFT = 0xA0,
            VK_CONTROL = 0xA2,
            VK_MENU = 0xA4,
            VK_CAPITAL = 0x14,
            VK_ALTGR = 0xA5
        }

        [System.Runtime.InteropServices.DllImport( "user32.dll" )]
        static extern void keybd_event( byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo );

        public static void Event( byte vk, byte scan, uint flags, UIntPtr extra )
        {
            keybd_event( vk, scan, flags, extra );
        }
    }
}
