using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WordPredictor.Model;
using CommonServices;

namespace CK.WordPredictor
{
    [Plugin( "{8789CDCC-A7BB-46E5-B119-28DC48C9A8B3}", PublicName = "Simple TextualContext aware predicted word sender", Description = "Listens to a successful prediction and prints the word, according to the current textual context.", Categories = new string[] { "Advanced" } )]
    public class SimpleTextualContextAwarePredictedWordSender : IPlugin
    {
        [RequiredService]
        public IWordPredictedService WordPredictedService { get; set; }

        [RequiredService]
        public ITextualContextService TextualContextService { get; set; }

        [RequiredService]
        public ICommandManagerService CommandManager { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        public bool SendSpaceAfterWordPredicted
        {
            get { return Config.User.TryGet( "SendSpaceAfterWordPredicted", true ); }
        }

        protected virtual void OnWordPredictionSuccessful( object sender, WordPredictionSuccessfulEventArgs e )
        {
            if( TextualContextService != null )
            {
                int currentContextTokenLenth = TextualContextService.CurrentToken.Value.Length;
                string wordToSend = e.Word.Substring( currentContextTokenLenth, e.Word.Length - currentContextTokenLenth );
                CommandManager.SendCommand( this, String.Concat( "sendString:", wordToSend ) );
                if( SendSpaceAfterWordPredicted ) CommandManager.SendCommand( this, "sendKey\" \"" );
            }
        }


        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            if( WordPredictedService != null )
                WordPredictedService.WordPredictionSuccessful += OnWordPredictionSuccessful;
        }

        public void Stop()
        {
            WordPredictedService.WordPredictionSuccessful -= OnWordPredictionSuccessful;
        }

        public void Teardown()
        {
        }
    }
}
