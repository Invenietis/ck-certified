#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\CommonServices\Advanced\Commands\BasicCommandHandler.cs) is part of CiviKey. 
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
    /// <summary>
    /// Provides a basic implementation for a CommandHandler. It manages the event registering
    /// in Start & Stop and the link to the Command Manager.
    /// </summary>
    public class BasicCommandHandler : IPlugin
    {
        bool _cLink;

        ICommandManagerService _cm;
        [DynamicService(Requires = RunningRequirement.MustExistAndRun )]
        public ICommandManagerService CommandManager
        {
            get { return _cm; }
            set
            {
                if( _cm != null && value == null )
                    StopListenCommandManager();

                _cm = value;

                if( _cm != null )
                    StartListenCommandManager();
            }
        }

        /// <summary>
        /// Method reacting from Command Sending event. Here, you can parse the command you want to listen
        /// before the command is really launches and do some stuff about it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"><see cref="CommandSendingEventArgs"/> containing data about the command which
        /// is about to be send.</param>
        protected virtual void OnCommandSending( object sender, CommandSendingEventArgs e ) { }

        /// <summary>
        /// Method reacting from the Command Sent event. Here, you can parse the command you want to listen
        /// and do some stuff about it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"><see cref="CommandSentEventArgs"/> containing data about the sent command.</param>
        protected virtual void OnCommandSent( object sender, CommandSentEventArgs e ) { }

        /// <summary>
        /// When the Optional CommandManager is set we listen its CommandSent event.
        /// </summary>
        void StartListenCommandManager()
        {
            _cm.CommandSent += new EventHandler<CommandSentEventArgs>( OnCommandSent );
            _cm.CommandSending += new EventHandler<CommandSendingEventArgs>( OnCommandSending );
            _cLink = true;
        }

        /// <summary>
        /// When the Optional CommandManager is unset we stop to listen its CommandSent event.
        /// </summary>
        void StopListenCommandManager()
        {
            _cm.CommandSent -= new EventHandler<CommandSentEventArgs>( OnCommandSent );
            _cm.CommandSending -= new EventHandler<CommandSendingEventArgs>( OnCommandSending );
            _cLink = false;
        }

        public virtual bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public virtual void Start()
        {
            if( _cm != null && !_cLink ) StartListenCommandManager();
        }

        public virtual void Stop()
        {
            if( _cm != null && _cLink ) StopListenCommandManager();
        }

        public virtual void Teardown()
        {
        }
    }
}
