using CK.Plugin;
using CommonServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace FileLauncher
{
    [Plugin(FileLauncher.PluginIdString,
           PublicName = PluginPublicName,
           Version = FileLauncher.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" })]
    public class FileLauncher : BasicCommandHandler
    {
        const string PluginIdString = "{02D08D49-171F-454A-A84C-89DD7F959958}";
        Guid PluginGuid = new Guid(PluginIdString);
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "File Launcher";
        const string CMD = "launch";
        Applications _win;

        public override void Start()
        {
            base.Start();
            FileLocator.Init();
            _win = new Applications() 
            {
                DataContext = new ApplicationViewModel()
                {
                    Apps = FileLocator.RegistryApps
                } 
            };
            Console.WriteLine("Oh lowl " + Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
            
            _win.Show();
        }

        protected override void OnCommandSent(object sender, CommandSentEventArgs e)
        {
            Command cmd = new Command(e.Command);
            if (cmd.Name != CMD) return;
            var f = LoadFile(cmd.Content);
            if (f != null) Launch(f);
        }

        public WildFile LoadFile(string token)
        {
            var info = token.Split(',');
            if(info.Length < 2 ) return null;

            WildFile f = new WildFile(info[Command.Contents.FILE_NAME]);
            f.Lookup = (FileLookup)int.Parse(info[Command.Contents.FILE_LOOKUP]);
            if (f.Lookup == FileLookup.Other && info.Length > 2)
            {
                f.Path = info[Command.Contents.FILE_PATH];
            }
            else if (f.Lookup == FileLookup.SpecialFolder && info.Length > 3)
            {
                f.Path = info[Command.Contents.FILE_PATH];
                f.FolderLocationType = (Environment.SpecialFolder)int.Parse(info[Command.Contents.FILE_SPECIAL_DIRECTORY]);
            }
            FileLocator.TryLocate(f);

            return f;
        }

        public void Launch(WildFile f)
        {
            try
            {
                Process.Start(f.Path);
            }
            catch (Win32Exception e) { } //Exception can be thrown just by clic on the cancel button of an install shield
        }
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

        public Command(string cmd)
        {
            int pos = cmd.IndexOf(SeparationToken);
            if (pos < 0) return;

            Name = cmd.Substring(0, pos);
            Content = cmd.Substring(pos + SeparationToken.Length);
        }
    }
}
