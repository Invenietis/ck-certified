#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\PointerDeviceDriver\DriverLib\WindowsHook.cs) is part of CiviKey. 
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

//****************************************************
// Author : Isaac Duplan (Duplan@intechinfo.fr)
// Date : 01-05-2008
//****************************************************
using System;
using System.Diagnostics;
using System.Threading;

namespace PointerDeviceDriver
{
    internal class WindowsHook : IDisposable
    {
        internal delegate void HookEventHandler(object sender, HookEventArgs e);

        // Raised at each Hook Event
        internal HookEventHandler HookProcInvoked;

        // Type of the Windows Hook

        IntPtr _hookHandle;
		readonly SynchronizationContext _syncCtx;
		readonly HookProc _hookProcHandle;
		readonly HookType _hookType;

        // Value which stop Hook chain when returned by the HookProc.
        private const int CANCEL_VALUE = 10;

        internal WindowsHook( SynchronizationContext syncCtx, HookType hType )
        {
            _hookType = hType;
            _hookProcHandle = new HookProc( MainHookProc );
			_syncCtx = syncCtx;
        }

        /// <summary>
        /// Sets the hook and returns the handle as an <see cref="IntPtr"/>.
		/// Can be called multiple times.
        /// </summary>
        internal IntPtr SetWindowsHook()
        {
			if( _hookHandle == IntPtr.Zero )
			{
				using( Process curProcess = Process.GetCurrentProcess() )
				{
					using( ProcessModule curModule = curProcess.MainModule )
					{
						_hookHandle = Win32Wrapper.SetWindowsHookEx( 
							_hookType, 
							_hookProcHandle, 
							Win32Wrapper.GetModuleHandle( curModule.ModuleName ), 0 );
					}
				}
			}
            return _hookHandle;
        }

        /// <summary>
        /// Returns true if the hook can be set, false if it can't be set.
        /// </summary>
        static internal bool CanSetWindowsHook( HookType type, out string errorMessage )
        {
			errorMessage = null;
			try
			{
				using( Process curProcess = Process.GetCurrentProcess() )
				{
					using( ProcessModule curModule = curProcess.MainModule )
					{
						IntPtr hookHandle = Win32Wrapper.SetWindowsHookEx( 
							type, 
							delegate( int code, IntPtr wParam, IntPtr lParam ) { return 0; }, 
							Win32Wrapper.GetModuleHandle( curModule.ModuleName ), 0 );

						if( hookHandle != null )
						{
							Win32Wrapper.UnhookWindowsHookEx( hookHandle.ToInt32() );
							return true;
						}
						errorMessage = "SetWindowsHookEx returned null.";
					}
				}
			}
			catch( Exception ex )
			{
				errorMessage = ex.Message;
			}
            return false;
        }

        /// <summary>
        /// Main HookProc: chain the call if not canceled by <see cref="HookProcInvoked"/>.
        /// </summary>
        private int MainHookProc( int code, IntPtr wParam, IntPtr lParam )
        {
            //Debug.WriteLine( "HookProcInvoked " );
            if( code < 0 ) return Win32Wrapper.CallNextHookEx( _hookHandle, code, wParam, lParam );

            HookEventArgs e = new HookEventArgs(code, wParam, lParam);
            if( HookProcInvoked != null ) HookProcInvoked( this, e );
			if( e.Cancel ) return CANCEL_VALUE;
            return Win32Wrapper.CallNextHookEx( _hookHandle, code, wParam, lParam );
        }

        /// <summary>
        /// Removes the current hook set by <see cref="SetWindowsHook"/>.
        /// </summary>
		public void Dispose()
		{
			if( _hookHandle != null )
			{
				Win32Wrapper.UnhookWindowsHookEx( _hookHandle.ToInt32() );
				_hookHandle = IntPtr.Zero;
			}
		}
	}

    internal class HookEventArgs : EventArgs
    {
        public readonly int Code;
        public readonly IntPtr wParam;
        public readonly IntPtr lParam;
        private bool _cancel;

        internal HookEventArgs(int Code, IntPtr wParam, IntPtr lParam)
        {
            this.Code = Code;
            this.wParam = wParam;
            this.lParam = lParam;
        }

        internal bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

    }
}
