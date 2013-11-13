using CK.Plugin;
using CommonServices;
using System;
using System.Collections.Generic;
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
        List<FileInfo> _apps;

        public override void Start()
        {
            base.Start();
            _apps = RegistryApplication.Crawl();
        }

        protected override void OnCommandSent(object sender, CommandSentEventArgs e)
        {
            Command cmd = new Command(e.Command);
            if (cmd.Name != CMD) return;
            var f = _apps.Where(a => a.FileName.Equals( cmd.Content, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (f != null) RegistryApplication.Launch(f);
        }
    }

    public class Command
    {
        static readonly string SeparationToken = ":";
        public string Name { get; private set; }

        public string Content { get; private set; }

        public Command(string cmd)
        {
            int pos = cmd.IndexOf(SeparationToken);
            Name = cmd.Substring(0, pos);
            Content = cmd.Substring(pos + SeparationToken.Length);
        }
    }
}
