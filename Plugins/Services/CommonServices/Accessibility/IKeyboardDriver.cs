#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\CommonServices\Accessibility\IKeyboardDriver.cs) is part of CiviKey. 
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
using CK.Plugin;

namespace CommonServices
{
    public interface IKeyboardDriver : IDynamicService
    {
        /// <summary>
        /// Fired at each keyboard event
        /// </summary>
        event EventHandler<KeyboardDriverEventArg> KeyDown;

        /// <summary>
        /// The given keycode will be automatically cancelled by the hook.
        /// </summary>
        /// <param name="keyCode"></param>
        void RegisterCancellableKey( int keyCode );

        void UnregisterCancellableKey( int keyCode );
    }

    public class KeyboardDriverEventArg : EventArgs
    {
        public KeyboardDriverEventArg( int keyCode, InputSource inputSource )
        {
            KeyCode = keyCode;
            InputSource = inputSource;
        }

        public int KeyCode { get; private set; }
        public InputSource InputSource { get; private set; }
    }

    public enum InputSource
    {
        Unknown = 0,
        CiviKey = 1,
        Other = 2
    }
}
