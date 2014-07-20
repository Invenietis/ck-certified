using CK.Plugin;
using CommonServices;
using System;
using ProtocolManagerModel;
using BasicCommandHandlers.Resources;
using CK.Plugin.Config;
using CK.Plugins.SendInputDriver;

namespace BasicCommandHandlers
{
    [Plugin( TextTemplateCommandHandler.PluginIdString,
           PublicName = PluginPublicName,
           Version = TextTemplateCommandHandler.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class TextTemplateCommandHandler : BasicCommandHandler, ITextTemplateCommandHandlerService
    {
        const string PluginIdString = "{78D84978-7A59-4211-BE04-DD25B5E2FDC1}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Text template Command Handler";

        const string PROTOCOL_BASE = "placeholder";
        const string PROTOCOL = PROTOCOL_BASE + ":";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IProtocolEditorsManager> ProtocolManagerService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITextTemplateService> TextTemplate { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ISendStringService> SendString { get; set; }

        [ConfigurationAccessor( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" )]
        public IPluginConfigAccessor SkinConfiguration { get; set; }

        public override void Start()
        {
            base.Start();
            ProtocolManagerService.Service.Register(
                                        new VMProtocolEditorMetaData(
                                        PROTOCOL_BASE,
                                        R.TextTemplateTitle,
                                        R.TextTemplateDescription,
                                        () => { return new TextTemplateCommandParameterManager( TextTemplate.Service, SendString.Service ); } ),
                                        typeof( ITextTemplateCommandHandlerService ) );
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

            OpenEditor( cmd.Content );
        }

        #region ITextTemplateCommandHandlerService Members

        public void OpenEditor( string template )
        {
            TextTemplate.Service.OpenEditor( template );
        }

        #endregion
    }

    public class Command
    {
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
