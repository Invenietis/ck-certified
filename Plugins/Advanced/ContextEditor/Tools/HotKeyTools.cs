#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Tools\HotKeyTools.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Runtime.InteropServices;

namespace KeyboardEditor.Tools
{
    public static class HotKeyHook
    {
        [DllImport( "user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall )]
        private static extern bool RegisterHotKey( IntPtr hWnd, int id, int fsModifiers, int vk );

        [DllImport( "user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall )]
        private static extern bool UnregisterHotKey( IntPtr hWnd, int id );

        private static int GetId( IntPtr hWnd, int modifier, int key )
        {
            return modifier ^ key ^ hWnd.ToInt32();
        }

        public static bool Register( IntPtr hWnd, int modifier, int key, out int id )
        {
            id = GetId( hWnd, modifier, key );
            return RegisterHotKey( hWnd, id, modifier, key );
        }

        public static bool Unregister( IntPtr hWnd, int modifier, int key )
        {
            return Unregister( hWnd, GetId( hWnd, modifier, key ) );
        }

        public static bool Unregister( IntPtr hWnd, int id )
        {
            return UnregisterHotKey( hWnd, id );
        }
    }

    public static class Constants
    {
        //modifiers
        public const int NOMOD = 0x0000;
        public const int ALT = 0x0001;
        public const int CTRL = 0x0002;
        public const int SHIFT = 0x0004;
        public const int WIN = 0x0008;

        //windows message id for hotkey
        public const int WM_HOTKEY_MSG_ID = 0x0312;
    }

    public class HookKey : IEquatable<HookKey>
    {
        public int Modifiers { get; private set; }
        public int KeyCode { get; private set; }

        public HookKey( int modifiers, int keyCode )
        {
            Modifiers = modifiers;
            KeyCode = keyCode;
        }

        public bool Equals( HookKey other )
        {
            return this.KeyCode == other.KeyCode && this.Modifiers == other.Modifiers;
        }
    }

    public class HookInvokedEventArgs
    {
        public int Message { get; private set; }
        public IntPtr WParam { get; private set; }
        public IntPtr LParam { get; private set; }

        public HookInvokedEventArgs( int message, IntPtr lParam, IntPtr wParam )
        {
            Message = message;
            WParam = wParam;
            LParam = lParam;
        }
    }
}
