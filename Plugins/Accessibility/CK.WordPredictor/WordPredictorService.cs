using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CK.Context;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WordPredictor.Engines;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    [Plugin( "{1764F522-A9E9-40E5-B821-25E12D10DC65}", PublicName = "WordPredictor", Categories = new[] { "Accessibility" } )]
    public class WordPredictorService : IPlugin, IWordPredictorService
    {
        IWordPredictorEngine _engine;
        ObservableCollection<IWordPredicted> _predictedList;
        WordPredictedCollection _wordPredictedCollection;

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

        public IWordPredictedCollection Words
        {
            get { return _wordPredictedCollection; }
        }

        Func<string> _resourcePath = () => Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ) );

        internal Func<string> PluginDirectoryPath
        {
            get { return _resourcePath; }
            set { _resourcePath = value; }
        }

        internal IWordPredictorEngine CurrentEngine
        {
            get { return _engine ?? (_engine = new WordPredictorEngineFactory( PluginDirectoryPath() ).Create( PredictorEngine )); }
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _predictedList = new ObservableCollection<IWordPredicted>();
            _wordPredictedCollection = new WordPredictedCollection( _predictedList );
            TextualContextService.PropertyChanged += TextualContextService_PropertyChanged;
        }

        void TextualContextService_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "CurrentToken" )
            {
                _predictedList.Clear();
                IEnumerable<IWordPredicted> words = CurrentEngine.Predict( TextualContextService, MaxSuggestedWords );
                foreach( var w in words ) _predictedList.Add( w );
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
