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
* Copyright © 2007-2014, 
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
using CK.InputDriver.Hook;
using CK.Plugin;
using CommonServices;

namespace PointerDeviceDriver
{
    /// <summary>
    /// 
    /// Implementation of IPointerDeviceDriver used to Drive the Mouse in a Windows environement
    /// </summary>
    [Plugin( "{CD792CE7-9ABA-4177-858C-AF7BA5D8D5B3}", PublicName = "Pointer DeviceDriver", Version = "1.0",
     Categories = new string[] { "Advanced" },
     Description = "Plugin that enables catching MouseMove events." )]
    public class MouseDriver : IPlugin, IPointerDeviceDriver
    {
        MouseHook _m;
        PointStruct _lastPointerPosition;
        
        public event WheelActionEventHandler WheelAction;
        //public event PointerDeviceEventHandler PointerButtonDoubleClick;
        public event PointerDeviceEventHandler PointerButtonDown;
        public event PointerDeviceEventHandler PointerButtonUp;
        public event PointerDeviceEventHandler PointerMove;

        public int CurrentPointerXLocation
        {
            get
            {
                return _lastPointerPosition.X;
            }
        }

        public int CurrentPointerYLocation
        {
            get
            {
                return _lastPointerPosition.Y;
            }
        }

        // Definition of the ExtraInfo used by this implementation
        public class ButtonExtraInfo
        {
            public const string Right = "Right";
            public const string Middle = "Middle";
            public const string Unknown = "Unknown";
        }

        public bool Setup( IPluginSetupInfo info )
        {
            _lastPointerPosition = new PointStruct();
            return true;
        }

        public void Start()
        {
            _m = new MouseHook();
            _m.SetWindowsHook();
            _m.OnHookProc = OnHookInvoqued;
        }

        public void Stop()
        {
        }

        public void Teardown()
        {

        }

        /// <summary>
        /// Method which provides an interpretation for all hook events.
        /// Depending of the hook's params we'll fire the good event.
        /// </summary>
        private void OnHookInvoqued( object sender, MouseArgs e )
        {
            InputSource source;
            if ( (int)e.Infos.dwExtraInfo == 39229115 ) //CiviKey's footprint
                source = InputSource.CiviKey;
            else
                source = InputSource.Other;

            int x = e.Infos.pt.X;
            int y = e.Infos.pt.Y;

            switch ( e.MouseMessage )
            {
                case CK.InputDriver.Native.MouseMessage.WM_LBUTTONDBLCLK:
                    //if ( PointerButtonDoubleClick != null )
                    //{
                    //    PointerDeviceEventArgs args = new PointerDeviceEventArgs( x, y, ButtonInfo.DefaultButton, String.Empty, source );
                    //    PointerButtonDoubleClick( this, args );
                    //}
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_LBUTTONDOWN:
                    if ( PointerButtonDown != null )
                    {
                        PointerDeviceEventArgs args = new PointerDeviceEventArgs( x, y, ButtonInfo.DefaultButton, String.Empty, source );
                        PointerButtonDown( this, args );
                    }
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_LBUTTONUP:
                    if ( PointerButtonUp != null )
                    {
                        PointerDeviceEventArgs args = new PointerDeviceEventArgs( x, y, ButtonInfo.DefaultButton, String.Empty, source );
                        PointerButtonUp( this, args );
                    }
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_MBUTTONDBLCLK:
                    //if ( PointerButtonDoubleClick != null )
                    //{
                    //    PointerDeviceEventArgs args = new PointerDeviceEventArgs( x, y, ButtonInfo.XButton, ButtonExtraInfo.Middle, source );
                    //    PointerButtonDoubleClick( this, args );
                    //}
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_MBUTTONDOWN:
                    if ( PointerButtonDown != null )
                    {
                        PointerDeviceEventArgs args = new PointerDeviceEventArgs( x, y, ButtonInfo.XButton, ButtonExtraInfo.Middle, source );
                        PointerButtonDown( this, args );
                    }
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_MBUTTONUP:
                    if ( PointerButtonUp != null )
                    {
                        PointerDeviceEventArgs args = new PointerDeviceEventArgs( x, y, ButtonInfo.XButton, ButtonExtraInfo.Middle, source );
                        PointerButtonUp( this, args );
                    }
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_MOUSEHWHEEL:
                    if ( WheelAction != null )
                    {
                        WheelActionEventArgs args = new WheelActionEventArgs( x, y, ButtonInfo.XButton, ButtonExtraInfo.Middle, source, e.Infos.mouseData );
                        WheelAction( this, args );
                    }
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_MOUSEMOVE:
                    _lastPointerPosition.X = x;
                    _lastPointerPosition.Y = y;
                    if ( PointerMove != null )
                    {
                        PointerDeviceEventArgs args = new PointerDeviceEventArgs( x, y, ButtonInfo.None, String.Empty, source );
                        PointerMove( this, args );
                    }
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_MOUSEWHEEL:
                    if ( WheelAction != null )
                    {
                        WheelActionEventArgs args = new WheelActionEventArgs( x, y, ButtonInfo.XButton, ButtonExtraInfo.Middle, source, e.Infos.mouseData );
                        WheelAction( this, args );
                    }
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_RBUTTONDBLCLK:
                    //if ( PointerButtonDoubleClick != null )
                    //{
                    //    PointerDeviceEventArgs args = new PointerDeviceEventArgs( x, y, ButtonInfo.XButton, ButtonExtraInfo.Right, source );
                    //    PointerButtonDoubleClick( this, args );
                    //}
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_RBUTTONDOWN:
                    if ( PointerButtonDown != null )
                    {
                        PointerDeviceEventArgs args = new PointerDeviceEventArgs( x, y, ButtonInfo.XButton, ButtonExtraInfo.Right, source );
                        PointerButtonDown( this, args );
                    }
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_RBUTTONUP:
                    if ( PointerButtonUp != null )
                    {
                        PointerDeviceEventArgs args = new PointerDeviceEventArgs( x, y, ButtonInfo.XButton, ButtonExtraInfo.Right, source );
                        PointerButtonUp( this, args );
                    }
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_XBUTTONDBLCLK:
                    //if ( PointerButtonDoubleClick != null )
                    //{
                    //    PointerDeviceEventArgs args = new PointerDeviceEventArgs( x, y, ButtonInfo.XButton, ButtonExtraInfo.Unknown, source );
                    //    PointerButtonDoubleClick( this, args );
                    //}
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_XBUTTONDOWN:
                    if ( PointerButtonDown != null )
                    {
                        PointerDeviceEventArgs args = new PointerDeviceEventArgs( x, y, ButtonInfo.XButton, ButtonExtraInfo.Unknown, source );
                        PointerButtonDown( this, args );
                    }
                    break;
                case CK.InputDriver.Native.MouseMessage.WM_XBUTTONUP:
                    if ( PointerButtonUp != null )
                    {
                        PointerDeviceEventArgs args = new PointerDeviceEventArgs( x, y, ButtonInfo.XButton, ButtonExtraInfo.Unknown, source );
                        PointerButtonUp( this, args );
                    }
                    break;
                default:
                    break;
            }

            //if ( args != null && args.Cancel )
            //    e.Cancel = true;
        }
    }
}
