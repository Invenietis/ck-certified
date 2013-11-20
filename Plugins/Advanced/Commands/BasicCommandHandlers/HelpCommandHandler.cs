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
using CommonServices.Accessibility;
using Help.Services;

namespace BasicCommandHandlers
{
    [Plugin( "{B982F488-4F67-43EB-A558-6487356AB5F4}", Categories = new string[] { "Advanced" },
        PublicName = "Help command handler",
        Version = "1.0.0" )]
    public class HelpCommandHandler : BasicCommandHandler, IHelpCommandHandlerService
    {
        const string PROTOCOL = "help:";
        IVersionedUniqueId skinUniqueId = new SimpleVersionedUniqueId( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}", new Version( "1.5.0" ) );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IHelpViewerService> HelpService { get; set; }

        //[DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        //public IService<ISkinService> SkinService { get; set; }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( e.Command.StartsWith( PROTOCOL ) )
            {
                string parameter = e.Command.Substring( PROTOCOL.Length ).Trim();

                if( parameter == "show" ) ShowHelp();
            }
        }

        public void ShowHelp()
        {
            if( HelpService.Status == InternalRunningStatus.Started ) HelpService.Service.ShowHelpFor( skinUniqueId, true );
        }
    }
}
