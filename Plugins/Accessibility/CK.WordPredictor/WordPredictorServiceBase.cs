using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CK.Context;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WordPredictor.Engines;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    public abstract class WordPredictorServiceBase : IPlugin, IWordPredictorService
    {
        ObservableCollection<IWordPredicted> _predictedList;
        WordPredictedCollection _wordPredictedCollection;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ITextualContextService TextualContextService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExist )]
        public IWordPredictorFeature Feature { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        public bool IsWeightedPrediction
        {
            get { return _engine != null ? _engine.IsWeightedPrediction : false; }
        }

        protected abstract string PredictorEngine { get; }

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

        IWordPredictorEngine _engine;
        Task _asyncEngineContinuation;

        internal Task AsyncEngineContinuation
        {
            get { return _asyncEngineContinuation; }
        }

        private void FeedPredictedList()
        {
            _predictedList.Clear();
            IEnumerable<IWordPredicted> words = _engine.Predict( TextualContextService, Feature.MaxSuggestedWords );
            foreach( var w in words ) _predictedList.Add( w );
        }

        public bool Setup( IPluginSetupInfo info )
        {
            try
            {
                _predictedList = new ObservableCollection<IWordPredicted>();
                _wordPredictedCollection = new WordPredictedCollection( _predictedList );

                var asyncEngine = new WordPredictorEngineFactory( PluginDirectoryPath() ).CreateAsync( PredictorEngine );
                _asyncEngineContinuation = asyncEngine.ContinueWith( task =>
                {
                    if( _engine == null ) _engine = task.Result;
                } );
                return true;
            }
            catch
            {
                return false;
            }

        }

        public void Start()
        {
            TextualContextService.PropertyChanged += TextualContextService_PropertyChanged;
        }

        void TextualContextService_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "CurrentToken" )
            {
                if( _engine == null ) _asyncEngineContinuation.ContinueWith( task => FeedPredictedList() );
                else FeedPredictedList();
            }
        }

        public void Stop()
        {
            _predictedList.Clear();
        }

        public void Teardown()
        {
            _engine = null;
        }
    }
}
