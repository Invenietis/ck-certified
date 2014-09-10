#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\KeySequence\KeySequenceCommandHandler.cs) is part of CiviKey. 
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

using CommonServices;
using CK.Plugin;
using ProtocolManagerModel;
using BasicCommandHandlers.Resources;

namespace BasicCommandHandlers
{
    public interface IKeySequenceCommandHandlerService : IDynamicService
    {
    }

    [Plugin( "{418F670B-46E8-4BE2-AF37-95F43040EEA6}", Categories = new string[] { "Advanced" },
        PublicName = "Key sequence command handler", Version = "2.0.0" )]
    public class KeySequenceCommandHandler : BasicCommandHandler, IKeySequenceCommandHandlerService
    {
        private const string PROTOCOL = "keysequence";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IProtocolEditorsManager> ProtocolManagerService { get; set; }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( !e.Canceled && e.Command.StartsWith( PROTOCOL ) )
            {
                string parameter = e.Command.Substring( e.Command.IndexOf( ':' ) + 1 );
                CK.InputDriver.Replay.KeyboardReplayer.Play( parameter.Split( ',' )[1] );
            }
        }

        public override void Start()
        {
            base.Start();
            ProtocolManagerService.Service.Register(
                                        new VMProtocolEditorMetaData(
                                        "keysequence",
                                        R.KeySequenceProtocolTitle,
                                        R.KeySequenceProtocolDescription,
                                        typeof( KeySequenceCommandParameterManager ) ),
                                        typeof( IKeySequenceCommandHandlerService ) );

            ProtocolManagerService.Service.Register(
                             new VMProtocolEditorMetaData(
                             "pause",
                             R.PauseTitle,
                             R.PauseDescription,
                             typeof( PauseCommandParameterManager ) ) );
        }

        public override void Stop()
        {
            ProtocolManagerService.Service.Unregister( "keysequence" );
            base.Stop();
        }
    }
}
