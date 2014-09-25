#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\ChangeKeyboard\ChangeKeyboard.cs) is part of CiviKey. 
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

using System.Linq;
using CK.Keyboard.Model;
using CK.Plugin;
using CommonServices;
using ProtocolManagerModel;
using BasicCommandHandlers.Resources;

namespace BasicCommandHandlers
{
    [Plugin( "{04B1B7F5-6CD8-4691-B5FD-2C4401C3AC0C}", Categories = new string[] { "Advanced" },
        PublicName = "Change keyboard command handler",
        Version = "1.0.0" )]
    public class ChangeKeyboardPlugin : BasicCommandHandler, IChangeKeyboardCommandHandlerService
    {
        const string PROTOCOL_BASE = "keyboardswitch";
        const string PROTOCOL = PROTOCOL_BASE + ":";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IProtocolEditorsManager> ProtocolManagerService { get; set; }

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
                var kb = KeyboardContext.Service.Keyboards[keyboardName];
                if( kb != null && KeyboardContext.Service.CurrentKeyboard != kb )
                {
                    KeyboardContext.Service.CurrentKeyboard = kb;
                }
            }
        }

        public override void Start()
        {
            base.Start();
            ProtocolManagerService.Service.Register(
                    new VMProtocolEditorMetaData( PROTOCOL_BASE,
                                                 R.ChangeKeyboard,
                                                 R.ChangeKeyboardDescription,
                                                 () => { return new ChangeKeyboardCommandParameterManager( KeyboardContext.Service.Keyboards.ToList() ); } ), 
                                                 typeof( IChangeKeyboardCommandHandlerService ) );

        }

        public override void Stop()
        {
            ProtocolManagerService.Service.Unregister( PROTOCOL_BASE );
            base.Stop();
        }
    }
}
