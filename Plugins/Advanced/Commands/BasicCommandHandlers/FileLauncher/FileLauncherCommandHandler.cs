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
