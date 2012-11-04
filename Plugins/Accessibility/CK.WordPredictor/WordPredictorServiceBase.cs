using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        static Func<string> _resourcePath = () => Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ) );

        ObservableCollection<IWordPredicted> _predictedList;
        WordPredictedCollection _wordPredictedCollection;
        IWordPredictorEngine _engine;
        Task _asyncEngineContinuation;

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public ITextualContextService TextualContextService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWordPredictorFeature Feature { get; set; }

        public bool IsWeightedPrediction
        {
            get { return _engine != null ? _engine.IsWeightedPrediction : false; }
        }

        public IWordPredictedCollection Words
        {
            get { return _wordPredictedCollection; }
        }

        protected abstract IWordPredictorEngineFactory EngineFactory { get; }

        protected virtual string PredictorEngine
        {
            get { return Feature.Engine; }
        }

        internal static Func<string> PluginDirectoryPath
        {
            get { return _resourcePath; }
            set { _resourcePath = value; }
        }

        internal Task AsyncEngineContinuation
        {
            get { return _asyncEngineContinuation; }
        }

        public virtual bool Setup( IPluginSetupInfo info )
        {
            try
            {
                _predictedList = new ObservableCollection<IWordPredicted>();
                _wordPredictedCollection = new WordPredictedCollection( _predictedList );

                return true;
            }
            catch
            {
                return false;
            }

        }

        public virtual void Start()
        {
            LoadEngine();
            TextualContextService.PropertyChanged += OnTextualContextServicePropertyChanged;
            Feature.PropertyChanged += OnFeaturePropertyChanged;
        }

        private void LoadEngine()
        {
            var asyncEngine = EngineFactory.CreateAsync( PredictorEngine );
            _asyncEngineContinuation = asyncEngine.ContinueWith( task =>
            {
                if( _engine == null ) _engine = task.Result;
            } );
        }

        protected virtual void OnFeaturePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "Engine" )
            {
                EngineFactory.Release( _engine );
                LoadEngine();
            }
        }

        protected virtual void OnTextualContextServicePropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "CurrentToken" )
            {
                if( _engine == null ) _asyncEngineContinuation.ContinueWith( task => FeedPredictedList() );
                else FeedPredictedList();
            }
        }

        private void FeedPredictedList()
        {
            _predictedList.Clear();
            IEnumerable<IWordPredicted> words = _engine.Predict( TextualContextService, Feature.MaxSuggestedWords );
            foreach( var w in words ) _predictedList.Add( w );
        }

        public virtual void Stop()
        {
            TextualContextService.PropertyChanged -= OnTextualContextServicePropertyChanged;

            EngineFactory.Release( _engine );
            _engine = null;
            _predictedList.Clear();
        }

        public virtual void Teardown()
        {
        }
    }
}
