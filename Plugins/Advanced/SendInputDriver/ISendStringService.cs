#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\SendInputDriver\ISendStringService.cs) is part of CiviKey. 
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
using CK.InputDriver;

namespace CK.Plugins.SendInputDriver
{
    public interface ISendStringService : IDynamicService
    {
        /// <summary>
        /// Send a String
        /// </summary>
        /// <param name="key">A classic string</param>
        void SendString( string key );

        /// <summary>
        /// Send a String
        /// </summary>
        /// <param name="key">A classic string</param>
        void SendString( object sender, string key );

        /// <summary>
        /// Send a specific keyboard key
        /// </summary>
        /// <param name="key"></param>
        void SendKeyboardKey( Native.KeyboardKeys key );

        /// <summary>
        /// Raised when a String is sent.
        /// </summary>
        event EventHandler<StringSentEventArgs> StringSent;

        /// <summary>
        /// Raised when a String is sending.
        /// </summary>
        event EventHandler<StringSendingEventArgs> StringSending;


        /// <summary>
        /// Raised when a Key is sent.
        /// </summary>
        event EventHandler<NativeKeySentEventArgs>  KeySent;

        /// <summary>
        /// Raised when a Key is sending.
        /// </summary>
        event EventHandler<NativeKeySendingEventArgs>  KeySending;
    }

    public class StringSentEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        public readonly string StringVal;

        public StringSentEventArgs( string s )
        {
            StringVal = s;
        }
    }

    /// <summary>
    /// </summary>
    public class StringSendingEventArgs : StringSentEventArgs
    {
        private bool _canceled;

        public bool Cancel
        {
            get { return _canceled; }
            set { _canceled = value; }
        }

        public StringSendingEventArgs( string s )
            : base( s )
        {
        }

    }


    public class NativeKeySentEventArgs : EventArgs
    {
        /// <summary>
        /// Store the keyword which represent the key which has been sent
        /// </summary>
        public readonly Native.KeyboardKeys Key;

        public NativeKeySentEventArgs(Native.KeyboardKeys key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// Contain the key which will be send to system, it's can be cancel
    /// Sample: the F1 key launch the help of focused application. we can block that
    /// </summary>
    public class NativeKeySendingEventArgs : NativeKeySentEventArgs
    {
        private bool _canceled;

        public bool Cancel
        {
            get { return _canceled; }
            set { _canceled = value; }
        }

        public NativeKeySendingEventArgs(Native.KeyboardKeys key)
            : base( key )
        {
        }

    }
}

