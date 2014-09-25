#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\CommonServices\Advanced\Commands\ICommandManagerService.cs) is part of CiviKey. 
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
using CK.Core;
using CK.Plugin;

namespace CommonServices
{
    public interface ICommandManagerService : IDynamicService
    {
        /// <summary>
        /// Event raised before the execution of the command.
        /// </summary>
        event EventHandler<CommandSendingEventArgs> CommandSending;

        /// <summary>
        /// Event raised to execute the command.
        /// </summary>
        event EventHandler<CommandSentEventArgs> CommandSent;

        /// <summary>
        /// Send commands in the application.
        /// Commands are automatically trimmed to ease the analysis.
        /// </summary>
        /// <param name="sender">The sender of the commands</param>
        /// <param name="commands">The commands</param>
        /// <exception cref="CommandException">When an error occurs. 
        /// The exception gives access to the command line that triggered the error.</exception>
        void SendCommands( object sender, ICKReadOnlyList<string> commands );

        void SendCommand( object sender, string command );

        /// <summary>
        /// Return true if we are running code resulting of a command execution
        /// </summary>
        bool IsRunningCommands { get; }
    }

    public class CommandSendingEventArgs : EventArgs
    {
        string _command;
        bool _cancel;

        /// <summary>
        /// Gets or sets the actual command send by the <see cref="CommandManager"/>.
        /// </summary>
        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }

        /// <summary>
        /// Gets or sets if the command have to be canceled or not. Defaults to false.
        /// </summary>
        public bool Canceled
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        public CommandSendingEventArgs( string cmd )
        {
            _command = cmd;
        }
    }

    public class CommandSentEventArgs : EventArgs
    {
        public string Command { get; private set; }
        public bool Canceled { get; private set; }

        public CommandSentEventArgs( CommandSendingEventArgs e )
        {
            Command = e.Command;
            Canceled = e.Canceled;
        }
    }
}
