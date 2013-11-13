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

        public override void Start()
        {
            base.Start();
            var list = RegistryApplication.Crawl();
            RegistryApplication.Launch(list.FirstOrDefault());
        }
    }
}
