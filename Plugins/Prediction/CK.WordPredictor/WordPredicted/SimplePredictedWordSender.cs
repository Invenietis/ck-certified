using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Plugins.SendInput;
using CK.WordPredictor.Model;
using CommonServices;

namespace CK.WordPredictor
{
    [Plugin( "{8789CDCC-A7BB-46E5-B119-28DC48C9A8B3}", PublicName = "Simple TextualContext aware predicted word sender", Description = "Listens to a successful prediction and prints the word, according to the current textual context.", Categories = new string[] { "Prediction" } )]
    public class SimplePredictedWordSender : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWordPredictedService WordPredictedService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IService<ITextualContextService> TextualContextService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IService<ISendStringService> SendStringService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWordPredictorFeature Feature { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        protected virtual void OnWordPredictionSuccessful( object sender, WordPredictionSuccessfulEventArgs e )
        {
            if( TextualContextService.Service != null && SendStringService.Service != null )
            {
                if( e.Word.Length > 0 && e.Word.Length > TextualContextService.Service.CaretOffset )
                {
                    string wordToSend = e.Word.Substring( TextualContextService.Service.CaretOffset, e.Word.Length - TextualContextService.Service.CaretOffset );

                    if( Feature.InsertSpaceAfterPredictedWord ) wordToSend += " ";

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
