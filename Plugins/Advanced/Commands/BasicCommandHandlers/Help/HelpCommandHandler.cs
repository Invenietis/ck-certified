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
using CommonServices;
using CK.Plugin;
using CK.Core;
using Help.Services;
using ProtocolManagerModel;
using BasicCommandHandlers.Resources;

namespace BasicCommandHandlers
{
    [Plugin( "{B982F488-4F67-43EB-A558-6487356AB5F4}", Categories = new string[] { "Advanced" },
        PublicName = "Help command handler",
        Version = "1.0.0" )]
    public class HelpCommandHandler : BasicCommandHandler, IHelpCommandHandlerService
    {
        const string PROTOCOL_BASE = "help";
        const string PROTOCOL = PROTOCOL_BASE + ":";
        IVersionedUniqueId skinUniqueId = new SimpleVersionedUniqueId( "{36c4764a-111c-45e4-83d6-e38fc1df5979}", new Version( "1.6.0" ) );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IHelpViewerService> HelpService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IProtocolEditorsManager> ProtocolManagerService { get; set; }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( e.Command.StartsWith( PROTOCOL ) )
            {
                string parameter = e.Command.Substring( PROTOCOL.Length ).Trim();

                if( parameter == "show" ) ShowHelp();
            }
        }

        public override void Start()
        {
            base.Start();
            ProtocolManagerService.Service.Register(
                                        new VMProtocolEditorWrapper(
                                        "help",
                                        R.HelpProtocolTitle,
                                        R.HelpProtocolDescription,
                                        typeof( HelpCommandParameterManager ) ),
                                        typeof( IHelpCommandHandlerService ) );
        }

        public void ShowHelp()
        {
            if( HelpService.Status == InternalRunningStatus.Started ) HelpService.Service.ShowHelpFor( skinUniqueId, true );
        }
    }
}
