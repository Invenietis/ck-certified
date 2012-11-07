using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BasicCommandHandlers;
using CK.Plugin;
using CK.Plugins.SendInput;
using CK.WordPredictor.Model;
using CommonServices;

namespace CK.WordPredictor
{
    [Plugin( "{B2A76BF2-E9D2-4B0B-ABD4-270958E17DA0}", PublicName = "TextualContext Command Handler", Categories = new string[] { "Prediction" } )]
    public class SendTextualContextCommandHandler : BasicCommandHandler, ISendTextualContextService
    {
        public const string CMDSendPredictedWord = "sendTextualContext";

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IService<ITextualContextService> TextualContextService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IService<ISendStringService> SendStringSevice { get; set; }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( e.Command == CMDSendPredictedWord )
                SendTextualContext( TextualContextService.Service );
        }

        public event EventHandler TextualContextSent;

        public void SendTextualContext( ITextualContextService textualContext )
        {
            if( !String.IsNullOrEmpty( TextualContextService.Service.RawContext ) )
            {
                SendStringSevice.Service.SendString( TextualContextService.Service.RawContext );
                if( TextualContextSent != null )
                    TextualContextSent( this, EventArgs.Empty );
            }
        }
    }
}
