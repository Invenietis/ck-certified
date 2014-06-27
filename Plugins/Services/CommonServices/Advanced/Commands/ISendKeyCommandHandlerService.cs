#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\CommonServices\Advanced\Commands\ISendKeyCommandHandlerService.cs) is part of CiviKey. 
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

namespace CommonServices
{
    public interface ISendKeyCommandHandlerService : IDynamicService
    {
        /// <summary>
        /// Send the key
        /// </summary>
        /// <param name="key">Keyword which represent the key</param>
        void SendKey( string key );

        /// <summary>
        /// Send the key and throw the sender to event
        /// </summary>
        /// <param name="key">Keyword which represent the key</param>
        void SendKey( object sender, string key );

        /// <summary>
        /// Raised when a Key is sent.
        /// </summary>
        event EventHandler<KeySentEventArgs> KeySent;

        /// <summary>
        /// Raised when a Key is sending
        /// </summary>
        event EventHandler<KeySendingEventArgs> KeySending;
    }

    public class KeySentEventArgs : EventArgs
    {
        /// <summary>
        /// Store the keyword which represent the key which has been sent
        /// </summary>
        public readonly string Key;

        public KeySentEventArgs( string key )
        {
            Key = key;
        }
    }

    /// <summary>
    /// Contain the key which will be send to system, it's can be cancel
    /// Sample: the F1 key launch the help of focused application. we can block that
    /// </summary>
    public class KeySendingEventArgs : KeySentEventArgs
    {
        private bool _canceled;

        public bool Cancel
        {
            get { return _canceled; }
            set { _canceled = value; }
        }

        public KeySendingEventArgs( string key )
            : base( key )
        {
        }
        
    }
}
