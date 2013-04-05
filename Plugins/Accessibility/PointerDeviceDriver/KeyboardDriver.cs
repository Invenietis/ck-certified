#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\PointerDeviceDriver\MouseDriver.cs) is part of CiviKey. 
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
using System.Threading;
using CommonServices;
using CK.Plugin;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Threading;

namespace PointerDeviceDriver
{
    /// <summary>
    /// Implementation of IPointerDeviceDriver used to Drive the Mouse in a Windows environement
    /// </summary>
    [Plugin( "{484FD138-A311-40F4-9482-37879D6A1F0E}", PublicName = "Keyboard driver", Version = "0.1",
     Categories = new string[] { "Advanced" },
     Description = "A plugin that catches keyboard events." )]
    public class KeyboardDriver : IPlugin, IKeyboardDriver
    {
        private const int WM_KEYDOWN = 0x0100;

        WindowsHook _windowsHook;
        SynchronizationContext _syncCtx;
        HashSet<int> _cancellableKeys;

        public event EventHandler<KeyboardDriverEventArg> KeyDown;
        
        public KeyboardDriver()
        {
            _syncCtx = SynchronizationContext.Current;
            if( _syncCtx == null )
            {
                // For tests purposes, we create a default SynchronizationContext to handle Post desynchronization:
                // unfortunately, this DOES NOT work the same as in a Windows Form application since it uses the ThreadPool
                // whereas the WindowsFormSynchronizationContext delegates the call to the message pump.
                _syncCtx = new SynchronizationContext();
            }
        }

        public void RegisterCancellableKey( int keyCode )
        {
            _cancellableKeys.Add( keyCode );
        }

        public void UnregisterCancellableKey( int keyCode )
        {
            _cancellableKeys.Remove( keyCode );
        }

        public bool Setup( IPluginSetupInfo info )
        {
            _cancellableKeys = new HashSet<int>();
            // We look if we can set the Low Level keyboard Hook 
            string message;
            if( !WindowsHook.CanSetWindowsHook( HookType.WH_KEYBOARD_LL, out message ) )
            {
                info.FailedDetailedMessage = "Unable to set the KeyboardHook, This Driver may only run on Windows. Error: " + message;
                return false;
            }
            return true;
        }

        public void Start()
        {
            // Build & Set a WH_KEYBOARD_LL (Low Level Mouse Windows Hook)
            _windowsHook = new WindowsHook( _syncCtx, HookType.WH_KEYBOARD_LL );
            _windowsHook.SetWindowsHook();
            _windowsHook.HookProcInvoked = OnHookInvoqued;
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
            if( _windowsHook != null )
            {
                // Event unregister
                _windowsHook.HookProcInvoked = null;
                // Unset the CurrentHook when we don't need it anymore
                _windowsHook.Dispose();
                _windowsHook = null;
            }
        }

        /// <summary>
        /// Method which provides an interpretation for all hook events.
        /// Depending of the hook's params we'll fire the good event.
        /// </summary>
        private void OnHookInvoqued( object sender, HookEventArgs e )
        {
            if( e.Code >= 0 && e.wParam == (IntPtr)WM_KEYDOWN )
            {
                int vkCode = Marshal.ReadInt32( e.lParam );
                if( _cancellableKeys.Contains( vkCode ) || _cancellableKeys.Contains( -1 ) )
                {
                    //We only swallow the event if we are going to do something with it
                    if(KeyDown != null) e.Cancel = true;
                    
                    Dispatcher.CurrentDispatcher.BeginInvoke( (Action<int>)FireEvent, vkCode );
                }
            }
        }

        void FireEvent( int vkCode )
        {
            KeyboardDriverEventArg eventArgs = new KeyboardDriverEventArg( vkCode );
            if( KeyDown != null ) KeyDown( this, eventArgs );
        }

    }
}
