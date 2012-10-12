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

namespace BasicCommandHandlers
{
    [Plugin( "{4EDBED5A-C38E-4A94-AD34-18720B09F3B7}", Categories = new string[] { "Advanced" },
        PublicName = "Clic command handler",
        Version = "1.0.0" )]
    public class ClicCommandHandler : BasicCommandHandler, IClicCommandHandlerService
    {
        const string PROTOCOL = "clics:";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> PointerDriver { get; set; }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( e.Command.StartsWith( PROTOCOL ) )
            {
                string parameter = e.Command.Substring( PROTOCOL.Length ).Trim();

                switch( parameter )
                {
                    case "simple":
                        Clic();
                        return;
                    case "right":
                        RightClic();
                        return;
                    case "double":
                        DoubleClic();
                        return;
                }
            }
        }

        #region IClicCommandHandlerService Members

        public void Clic()
        {
            PointerDriver.Service.SimulateButtonDown( ButtonInfo.DefaultButton, "" );
            PointerDriver.Service.SimulateButtonUp( ButtonInfo.DefaultButton, "" );
        }

        public void DoubleClic()
        {
            Clic();
            Clic();
        }

        public void RightClic()
        {
            PointerDriver.Service.SimulateButtonUp( ButtonInfo.XButton, "Right" );
            PointerDriver.Service.SimulateButtonUp( ButtonInfo.XButton, "Right" );
        }

        #endregion
    }
}
