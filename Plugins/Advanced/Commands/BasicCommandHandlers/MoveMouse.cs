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
    [Plugin( "{B2EC4D13-7A4F-4F9E-A713-D5F8DDD161EF}", Categories = new string[] { "Advanced" },
        PublicName = "Move mouse command handler",
        Description="Allows the system to execute simple actions for specific commands",
        Version = "1.0.0")]
    public class MoveMouseCommandHandler : BasicCommandHandler
    {
        const string PROTOCOL = "movemouse:";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> PointerDriver { get; set; }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( e.Command.StartsWith( PROTOCOL ) )
            {
                string[] parameters = e.Command.Substring( PROTOCOL.Length ).Trim().Split( ',' );
                string direction = parameters[0];
                int step = int.Parse( parameters[1] );

                MoveMouse( direction, step );
            }
        }

        void MoveMouse( string direction, int step )
        {
            switch( direction )
            {
                case "U":
                    PointerDriver.Service.MovePointer(
                        PointerDriver.Service.CurrentPointerXLocation,
                        PointerDriver.Service.CurrentPointerYLocation - step );
                    return;
                case "R":
                    PointerDriver.Service.MovePointer(
                        PointerDriver.Service.CurrentPointerXLocation + step,
                        PointerDriver.Service.CurrentPointerYLocation );
                    return;
                case "B":
                    PointerDriver.Service.MovePointer(
                        PointerDriver.Service.CurrentPointerXLocation,
                        PointerDriver.Service.CurrentPointerYLocation + step );
                    return;
                case "L":
                    PointerDriver.Service.MovePointer(
                        PointerDriver.Service.CurrentPointerXLocation - step,
                        PointerDriver.Service.CurrentPointerYLocation );
                    return;
            }

            double angle = 0.0;
            switch( direction )
            {
                case "UL":
                    angle = -((3 * Math.PI) / 4);
                    break;
                case "BL":
                    angle = (3 * Math.PI) / 4;
                    break;
                case "BR":
                    angle = Math.PI / 4;
                    break;
                case "UR":
                    angle = -(Math.PI / 4);
                    break;
            }

            int x = (int)(PointerDriver.Service.CurrentPointerXLocation + (step * Math.Cos( angle )));
            int y = (int)(PointerDriver.Service.CurrentPointerYLocation + (step * Math.Sin( angle )));
            PointerDriver.Service.MovePointer( x, y );
        }
    }
}
