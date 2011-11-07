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
