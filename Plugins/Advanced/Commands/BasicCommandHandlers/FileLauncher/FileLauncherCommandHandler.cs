#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\FileLauncher\FileLauncherCommandHandler.cs) is part of CiviKey. 
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

using CK.Plugin;
using CommonServices;
using System;
using ProtocolManagerModel;
using BasicCommandHandlers.Resources;
using CK.Plugin.Config;

namespace BasicCommandHandlers
{
    [Plugin( FileLauncherCommandHandler.PluginIdString,
           PublicName = PluginPublicName,
           Version = FileLauncherCommandHandler.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class FileLauncherCommandHandler : BasicCommandHandler, IFileLauncherCommandHandlerService
    {
        const string PluginIdString = "{664AF22C-8C0A-4112-B6AD-FB03CDDF1603}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "File Launcher Command Handler";

        const string PROTOCOL_BASE = "launch";
        const string PROTOCOL = PROTOCOL_BASE + ":";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IProtocolEditorsManager> ProtocolManagerService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IFileLauncherService> FileLauncher { get; set; }

        [ConfigurationAccessor( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" )]
        public IPluginConfigAccessor SkinConfiguration { get; set; }

        public override void Start()
        {
            base.Start();
            ProtocolManagerService.Service.Register(
                                        new VMProtocolEditorMetaData(
                                        PROTOCOL_BASE,
                                        R.FileLauncherTitle,
                                        R.FileLauncherDescription,
                                        () => { return new FileLauncherCommandParameterManager( FileLauncher.Service, SkinConfiguration ); } ),
                                        typeof( IFileLauncherCommandHandlerService ) );
        }

        public override void Stop()
        {
            ProtocolManagerService.Service.Unregister( PROTOCOL_BASE );
            base.Stop();
        }
        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            Command cmd = new Command( e.Command );
            if( cmd.Name != PROTOCOL_BASE ) return;

            LaunchFile( cmd.Content );
        }

        #region IFileLauncherCommandHandlerService Members

        public void LaunchFile( string command )
        {
            FileLauncher.Service.LoadFromCommand( command, ( f ) =>
            {
                if( f != null ) FileLauncher.Service.Launch( f );
            } );
        }

        #endregion
        public class Command
        {
            public static class Contents
            {
                public static readonly int FILE_NAME = 0;
                public static readonly int FILE_LOOKUP = 1;
                public static readonly int FILE_PATH = 2;
                public static readonly int FILE_SPECIAL_DIRECTORY = 3;
            }

            static readonly string SeparationToken = ":";
            public string Name { get; private set; }
            public string Content { get; private set; }

            public Command( string cmd )
            {
                int pos = cmd.IndexOf( SeparationToken );
                if( pos < 0 ) return;

                Name = cmd.Substring( 0, pos );
                Content = cmd.Substring( pos + SeparationToken.Length );
            }
        }
    }
}
