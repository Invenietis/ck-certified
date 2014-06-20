#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\InputTrigger\InputListener.cs) is part of CiviKey. 
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
using CK.Plugin;
using CommonServices;

namespace InputTrigger
{
    public class InputListener : IInputListener
    {
        IService<IKeyboardDriver> _keboardDriver;
        IService<IPointerDeviceDriver> _mouseDriver;

        private  Action<ITrigger> _recordCallback;

        public bool IsRecording { get; private set; }

        public InputListener( IService<IKeyboardDriver> kb, IService<IPointerDeviceDriver> pd )
        {
            _keboardDriver = kb;
            _mouseDriver = pd;

            _keboardDriver.Service.KeyDown += OnKeyDown;
            _mouseDriver.Service.PointerButtonDown += OnMouseDown;
        }

        public event EventHandler<KeyDownEventArgs> KeyDown;

        public void Record( Action<ITrigger> callback )
        {
            IsRecording = true;
            _recordCallback = callback;
        }

        void OnKeyDown( object sender, KeyboardDriverEventArg e )
        {
            if( IsRecording )
            {
                Console.WriteLine( "Key down recorded" );
                _recordCallback( new Trigger( e.KeyCode, e.InputSource == InputSource.CiviKey ? TriggerDevice.Civikey : TriggerDevice.Keyboard ) );
                IsRecording = false;
            }
            else
            {
                FireKeyDown( e.KeyCode, e.InputSource == InputSource.CiviKey ? TriggerDevice.Civikey : TriggerDevice.Keyboard );
            }
        }

        void OnMouseDown( object sender, PointerDeviceEventArgs e )
        {
            if( IsRecording )
            {
                Console.WriteLine( "Mouse down recorded" );
                _recordCallback( new Trigger( MouseCodeFromButtonInfo( e.ButtonInfo, e.ExtraInfo ), TriggerDevice.Pointer ) );
                IsRecording = false;
            }
            else
            {
                FireKeyDown( MouseCodeFromButtonInfo( e.ButtonInfo, e.ExtraInfo ), e.Source == InputSource.CiviKey ? TriggerDevice.Civikey : TriggerDevice.Pointer );
            }
        }

        void FireKeyDown( int keyCode, TriggerDevice device )
        {
            if( KeyDown != null ) KeyDown( this, new KeyDownEventArgs( keyCode, device ) );
        }

        //JL : duplicate with MouseCodeFromButtonInfo in BasicScroll/Editor/EditorViewModel.cs. Where can it be put to avoid duplication ?
        public int MouseCodeFromButtonInfo( ButtonInfo buttonInfo, string extraInfo )
        {
            if( buttonInfo == ButtonInfo.DefaultButton )
            {
                return 1;
            }

            if( buttonInfo == ButtonInfo.XButton )
            {
                if( extraInfo == "Right" )
                    return 2;

                if( extraInfo == "Middle" )
                    return 3;
            }

            return 0;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _keboardDriver.Service.KeyDown -= OnKeyDown;
            _mouseDriver.Service.PointerButtonDown -= OnMouseDown;
        }

        #endregion
    }
}
