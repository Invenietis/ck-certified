using System;

namespace BasicCommandHandlers
{
    internal enum VKeyCode : byte
    {
        VK_LSHIFT = 0xA0,
        VK_RSHIFT = 0xA1,
        VK_CONTROL = 0xA2,
        VK_MENU = 0xA4,
        VK_CAPITAL = 0x14,
        VK_ALTGR = 0xA5,
        VK_WIN = 0x5B,
        VK_APPS = 0x5D,
        VK_CARET = 0xDD,
        VK_PERCENT = 0xC0,
        SC_ALTGR_FR = 0x38
    }

    internal class Keybd
    {
        public enum KEYEVENTF : uint
        {
            KEYDOWN = 0x0,
            EXTENDEDKEY = 0x1,
            KEYUP = 0x2
        }

        [System.Runtime.InteropServices.DllImport( "user32.dll" )]
        static extern void keybd_event( byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo );

        [System.Runtime.InteropServices.DllImport( "user32.dll" )]
        static extern short VkKeyScan( char c );

        public static void Event( VKeyCode vk, byte scan, KEYEVENTF flags, UIntPtr extra )
        {
            keybd_event( (byte)vk, scan, (byte)flags, extra );
        }

        public static short GetKeyCode( char c )
        {
            return VkKeyScan( c );
        }
    }
}
