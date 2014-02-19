using CK.Plugin;
using CommonServices;
using System;
using System.ComponentModel;
using System.Diagnostics;


namespace FileLauncher
{
    [Plugin( Launcher.PluginIdString,
           PublicName = PluginPublicName,
           Version = Launcher.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class Launcher : BasicCommandHandler, IFileLauncherService
    {
        const string PluginIdString = "{02D08D49-171F-454A-A84C-89DD7F959958}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "File Launcher";

        public override void Start()
        {
            FileLocator = new FileLocator();
            base.Start();
        }

        public void LoadFromCommand( string command, Action<IWildFile> loaded )
        {
            var info = command.Split( ',' );
            if( info.Length < 2 ) return;

            WildFile f = new WildFile( info[Command.Contents.FILE_NAME] );
            f.Lookup = (FileLookup)int.Parse( info[Command.Contents.FILE_LOOKUP] );
            if( (f.Lookup == FileLookup.Other || f.Lookup == FileLookup.Url) && info.Length > 2 )
            {
                f.Path = info[Command.Contents.FILE_PATH];
            }
            else if( f.Lookup == FileLookup.SpecialFolder && info.Length > 3 )
            {
                f.Path = info[Command.Contents.FILE_PATH];
                f.FolderLocationType = (Environment.SpecialFolder)int.Parse( info[Command.Contents.FILE_SPECIAL_DIRECTORY] );
            }

            FileLocator.TryLocate( f, loaded );
        }

        public void Launch( IWildFile f )
        {
            if( !f.IsLocated ) return;
            try
            {
                Process.Start( f.Path );
            }
            catch( Win32Exception e ) { } //Exception can be thrown just by clicking on the cancel button of an install wizard
        }

        #region IFileLauncherService Members

        public IFileLocator FileLocator { get; private set; }

        #endregion
    }

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
