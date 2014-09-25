#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\Tools\Keybd.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
