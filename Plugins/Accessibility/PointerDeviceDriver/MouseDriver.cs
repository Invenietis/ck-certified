#region LGPL License
/*----------------------------------------------------------------------------
* This file (StandardPlugins\PointerDeviceDriver\MouseDriver.cs) is part of CiviKey. 
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
* Copyright © 2007-2009, 
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
using System.Runtime.InteropServices;
using System.Threading;
using CommonServices;
using CK.Plugin;

namespace PointerDeviceDriver
{
	/// <summary>
	/// Implementation of IPointerDeviceDriver used to Drive the Mouse in a Windows environement
	/// </summary>
    [Plugin("{CD792CE7-9ABA-4177-858C-AF7BA5D8D5B3}", PublicName = "Pointer DeviceDriver", Version = "1.0",
     Categories = new string[] { "Advanced" },
     Description = "Plugin that enables one to simulate a click or to catch MouseMove events.")]
	public class MouseDriver : IPlugin, IPointerDeviceDriver
	{
		private int _X;
		private int _Y;

		readonly SynchronizationContext _syncCtx;
		WindowsHook _windowsHook;

		public event PointerDeviceEventHandler PointerMove;
		public event PointerDeviceEventHandler PointerButtonDown;
		public event PointerDeviceEventHandler PointerButtonUp;

        public int CurrentPointerXLocation { get { return _X; } }
        
        public int CurrentPointerYLocation { get { return _Y; } }

		// Definition of the ExtraInfo used by this implementation
		public class ButtonExtraInfo
		{
			public const string Right = "Right";
			public const string Middle = "Middle";
		}

		/// <summary>
		/// Default constructor, explicitely called by the Plugin Runner. 
		/// Initializes a <see cref="SynchronizationContext"/>.
		/// </summary>
		public MouseDriver()
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

        public bool Setup( IPluginSetupInfo info )
        {
            // We look if we can set the Low Level Mouse Hook 
            string message;
			if( WindowsHook.CanSetHook( HookType.WH_MOUSE_LL, out message ) ) return true;

			info.FailedDetailedMessage = "Unable to set the MouseHook, This Driver may only run on Windows. Error: " + message;
			return false;
        }

		public void Start()
		{
			// Build & Set a WH_MOUSE_LL (Low Level Mouse Windows Hook)
			_windowsHook = new WindowsHook( _syncCtx, HookType.WH_MOUSE_LL );
			_windowsHook.SetWindowsHook();
			_windowsHook.HookProcInvoked = OnHookInvoqued;
		}

		public void Stop()
		{
			// Event unregister
			_windowsHook.HookProcInvoked = null;
			// Unset the CurrentHook when we don't need it anymore
			_windowsHook.Dispose();
			_windowsHook = null;
		}

        public void Teardown()
        {
        }

		//TODO : Comment code
		public void MovePointer( int x, int y )
		{
			_syncCtx.Post( delegate( object p ) { DoMovePointer( (PointStruct)p ); }, new PointStruct( x, y ) );
		}

		//TODO : Comment code
		static void DoMovePointer( PointStruct p )
		{
			// Set cursor position to specified coordinates.
			Win32Wrapper.SetCursorPos( p.X, p.Y );
			// Throw a low level mouse move event to tell it to the world.
			Win32Wrapper.mouse_event( MouseEventFlags.MOVE, 0, 0, 0, 0 );
		}

		/// <summary>
		/// Simulate a ButtonDown Event
		/// </summary>
		/// <param name="buttonInfo">Default button is Left, look at ButtonInfo to see available buttons</param>
		public void SimulateButtonDown( ButtonInfo buttonInfo, string extraInfo )
		{
			if( buttonInfo == ButtonInfo.DefaultButton )
				Win32Wrapper.mouse_event( MouseEventFlags.LEFTDOWN, 0, 0, 0, 0 );

			if( buttonInfo == ButtonInfo.XButton && extraInfo == ButtonExtraInfo.Right )
				Win32Wrapper.mouse_event( MouseEventFlags.RIGHTDOWN, 0, 0, 0, 0 );

			if( buttonInfo == ButtonInfo.XButton && extraInfo == ButtonExtraInfo.Middle )
				Win32Wrapper.mouse_event( MouseEventFlags.MIDDLEDOWN, 0, 0, 0, 0 );
		}

		/// <summary>
		/// Simulate a ButtonUp Event
		/// </summary>
		/// <param name="buttonInfo">Default button is Left, look at ButtonInfo to see available buttons</param>
		public void SimulateButtonUp( ButtonInfo buttonInfo, string extraInfo )
		{
			if( buttonInfo == ButtonInfo.DefaultButton )
				Win32Wrapper.mouse_event( MouseEventFlags.LEFTUP, 0, 0, 0, 0 );

			if( buttonInfo == ButtonInfo.XButton && extraInfo == ButtonExtraInfo.Right )
				Win32Wrapper.mouse_event( MouseEventFlags.RIGHTUP, 0, 0, 0, 0 );

			if( buttonInfo == ButtonInfo.XButton && extraInfo == ButtonExtraInfo.Middle )
				Win32Wrapper.mouse_event( MouseEventFlags.MIDDLEUP, 0, 0, 0, 0 );
		}

		/// <summary>
		/// Method which provides an interpretation for all hook events.
		/// Depending of the hook's params we'll fire the good event.
		/// </summary>
		private void OnHookInvoqued( object sender, HookEventArgs e )
		{
			MouseHookAction action = (MouseHookAction)e.Code;
			LLMouseHookStruct mouseInfo = (LLMouseHookStruct)Marshal.PtrToStructure( e.lParam, typeof( LLMouseHookStruct ) );
			MouseMessage mouseMessage = (MouseMessage)e.wParam.ToInt32();

			// We only handle the hook if the hook proc contain informations
			if( action == MouseHookAction.HC_ACTION )
			{
				_X = mouseInfo.pt.X;
				_Y = mouseInfo.pt.Y;
				string buttonExtraInfo = String.Empty;
				ButtonInfo buttonInfo = GetButtonInfo( mouseMessage, out buttonExtraInfo );

				PointerDeviceEventArgs pointerEventArgs = new PointerDeviceEventArgs( _X, _Y, buttonInfo, buttonExtraInfo );

				// We look at the MouseMessage (wParam of the HookProc delegate) to know which event to fire.
				if( mouseMessage == MouseMessage.WM_MOUSEMOVE )
				{
                    //Console.Out.WriteLine( "Mouvement : " + DateTime.UtcNow );
					if( PointerMove != null ) PointerMove( this, pointerEventArgs );
				}
				else if( mouseMessage == MouseMessage.WM_LBUTTONDOWN ||
                    mouseMessage == MouseMessage.WM_MBUTTONDOWN ||
                    mouseMessage == MouseMessage.WM_RBUTTONDOWN )
				{
					if( PointerButtonDown != null ) PointerButtonDown( this, pointerEventArgs );
				}
				else if( mouseMessage == MouseMessage.WM_LBUTTONUP ||
                    mouseMessage == MouseMessage.WM_MBUTTONUP ||
                    mouseMessage == MouseMessage.WM_RBUTTONUP )
				{
					if( PointerButtonUp != null ) PointerButtonUp( this, pointerEventArgs );
				}

				// if a client wanted to cancel the current event we tell it to the WindowsHook procedure.
				if( pointerEventArgs.Cancel )
					e.Cancel = true;
			}

		}

		/// <summary>
		/// Method witch look at the MouseMessage(wParam of the HookProc delegate) to about which button this event is.
		/// </summary>
		private ButtonInfo GetButtonInfo( MouseMessage mouseMessage, out string extraInfo )
		{
			ButtonInfo button = ButtonInfo.None;
			extraInfo = String.Empty;

			if( mouseMessage == MouseMessage.WM_LBUTTONDOWN ||
                mouseMessage == MouseMessage.WM_LBUTTONUP ) button = ButtonInfo.DefaultButton;

			if( mouseMessage == MouseMessage.WM_RBUTTONDOWN ||
                mouseMessage == MouseMessage.WM_RBUTTONUP ) { button = ButtonInfo.XButton; extraInfo = ButtonExtraInfo.Right; }

			if( mouseMessage == MouseMessage.WM_MBUTTONDOWN ||
                mouseMessage == MouseMessage.WM_MBUTTONUP ) { button = ButtonInfo.XButton; extraInfo = ButtonExtraInfo.Middle; }

			if( mouseMessage == MouseMessage.WM_MOUSEMOVE ) button = ButtonInfo.None;

			return button;
		}

    }
}
