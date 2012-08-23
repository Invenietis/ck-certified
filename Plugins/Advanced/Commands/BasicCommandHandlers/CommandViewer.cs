#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\CommandViewer.cs) is part of CiviKey. 
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
using Host.Services;
using System;

namespace BasicCommandHandlers
{
    [Plugin(CommandViewer.PluginId, Categories = new string[] { "Development" },
        PublicName = "Command viewer", Version = "1.0.0" )]
    public class CommandViewer : BasicCommandHandler
    {
        const string PluginId = "{376ADF69-7D43-423D-93CE-30CB75B24069}";

        [RequiredService]
        public INotificationService Notifications { get; set; }

        public override void Start()
        {
            base.Start();
        }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( !e.Canceled )
                Notifications.ShowNotification( new Guid( PluginId ), "Command viewer : Command sent", "Command : " + e.Command, 1000 );
        }
    }
}
