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
    [Plugin("{0628E093-A9C6-4EC6-83C8-F684352B5B37}", Categories = new string[] { "Advanced" },
        PublicName = "MoveMouseCommandHandler",
        Description="Allows the system to execute simple actions for specific commands",
        Version = "1.0.0")]
    public class MoveMouseCommandHandler : BasicCommandHandler
    {
        const string PROTOCOL = "movemouse:";

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( e.Command.StartsWith( PROTOCOL ) )
            {
                Console.WriteLine( e.Command );
            }
        }
    }
}
