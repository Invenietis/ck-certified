using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CK.Context;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WordPredictor.Engines;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    [Plugin( "{1764F522-A9E9-40E5-B821-25E12D10DC65}", PublicName = "WordPredictor", Categories = new[] { "Accessibility" } )]
    public class WordPredictorService : IPlugin, IPredictorService
    {
        IWordPredictorEngine _engine;
        List<IWordPredicted> _predictedList;

        [RequiredService]
        public ITextualContextService TextualContextService { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        [RequiredService( Required = true )]
        public IContext Context { get; set; }

        public bool IsWeightedPrediction
        {
            get { return CurrentEngine.IsWeightedPrediction; }
        }

        protected int MaxSuggestedWords
        {
            get { return Config.User.TryGet( "MaxSuggestedWords", 10 ); }
        }

        protected string PredictorEngine
        {
            get { return Config.User.TryGet( "PredictorEngine", "sybille" ); }
        }

        public ReadOnlyObservableCollection<IWordPredicted> Words
        {
            get { return new ReadOnlyObservableCollection<IWordPredicted>( new ObservableCollection<IWordPredicted>( _predictedList ) ); }
        }

        internal string ResourcePath { get; set; }

        internal IWordPredictorEngine CurrentEngine
        {
            get { return _engine ?? (_engine = new WordPredictorEngineFactory( ResourcePath ).Create( PredictorEngine )); }
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _predictedList = new List<IWordPredicted>();
            TextualContextService.PropertyChanged += TextualContextService_PropertyChanged;
        }

        void TextualContextService_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "CurrentToken" )
            {
                IEnumerable<IWordPredicted> words = CurrentEngine.Predict( TextualContextService, MaxSuggestedWords );
                _predictedList.Clear();
                _predictedList.AddRange( words );
            }
        }

        public void Stop()
        {
            _predictedList.Clear();
        }

        public void Teardown()
        {
        }
    }
}
