#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\CommonServices\Advanced\Commands\Keybd.cs) is part of CiviKey. 
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
