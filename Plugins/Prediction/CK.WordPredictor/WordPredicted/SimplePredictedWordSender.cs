using System;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Plugins.SendInputDriver;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    [Plugin( "{8789CDCC-A7BB-46E5-B119-28DC48C9A8B3}", PublicName = "Simple TextualContext aware predicted word sender", Description = "Listens to a successful prediction and prints the word, according to the current textual context.", Categories = new string[] { "Prediction" }, Version="1.0" )]
    public class SimplePredictedWordSender : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWordPredictedService WordPredictedService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITextualContextService> TextualContextService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ISendStringService> SendStringService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWordPredictorFeature Feature { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        protected virtual void OnWordPredictionSuccessful( object sender, WordPredictionSuccessfulEventArgs e )
        {
            if( TextualContextService.Service != null && SendStringService.Service != null )
            {
                int caretOffset = TextualContextService.Service.CaretOffset;
                if( e.Word.Length > 0 && e.Word.Length > caretOffset )
                {
                    string wordToSend = e.Word;

                    if( TextualContextService.Service.CurrentToken != null 
                        && e.Word.Substring( 0, caretOffset ).Equals( TextualContextService.Service.CurrentToken.Value, StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        wordToSend = e.Word.Substring( caretOffset, e.Word.Length - caretOffset );
                    }

                    if( Feature.InsertSpaceAfterPredictedWord && !wordToSend.EndsWith( "'" ) ) wordToSend += " ";

                    SendStringService.Service.SendString( wordToSend );
                }
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
            if( WordPredictedService != null )
                WordPredictedService.WordPredictionSuccessful -= OnWordPredictionSuccessful;
        }

        public void Teardown()
        {
        }
    }
}
