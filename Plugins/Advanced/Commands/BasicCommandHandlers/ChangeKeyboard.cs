#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\DynCommandHandler.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using CommonServices;
using CK.Plugin;
using CK.Core;
using CK.Context;
using CK.Keyboard.Model;

namespace BasicCommandHandlers
{
    [Plugin( "{04B1B7F5-6CD8-4691-B5FD-2C4401C3AC0C}", Categories = new string[] { "Advanced" },
        PublicName = "Change keyboard command handler",
        Version = "1.0.0")]
    public class ChangeKeyboardPlugin : BasicCommandHandler, IChangeKeyboardCommandHandlerService
    {
        const string PROTOCOL = "keyboardswitch:";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardContext KeyboardContext { get; set; }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( e.Command.StartsWith( PROTOCOL ) )
            {
                string keyboardName = e.Command.Substring( PROTOCOL.Length );
                ChangeKeyboard( keyboardName );
            }
        }

        public void ChangeKeyboard( string keyboardName )
        {
            if( !string.IsNullOrEmpty( keyboardName ) )
            {
                var kb = KeyboardContext.Keyboards[keyboardName];
                if( kb != null && KeyboardContext.CurrentKeyboard != kb )
                {
                    KeyboardContext.CurrentKeyboard = kb;
                }
            }
        }
    }
}
