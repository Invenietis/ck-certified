using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CK.Plugin;
using CK.WordPredictor.Model;
using CK.Core;
using System.Linq;


namespace CK.WordPredictor
{
    public abstract class WordPredictorServiceBase : IPlugin, IWordPredictorService
    {
        static Func<string> _resourcePath = () => Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ) );

        FastObservableCollection<IWordPredicted> _predictedList;
        WordPredictedCollection _wordPredictedCollection;
        IWordPredictorEngine _engine;
        Task _asyncEngineContinuation;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ITextualContextService TextualContextService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWordPredictorFeature Feature { get; set; }

        public IWordPredictedCollection Words
        {
            get { return _wordPredictedCollection; }
        }

        protected abstract IWordPredictorEngineFactory EngineFactory { get; }

        internal protected static Func<string> PluginDirectoryPath
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

            return true;
        }

        public virtual void Start()
        {
            List<IWordPredicted> internalList = new List<IWordPredicted>( Feature.MaxSuggestedWords );
            _predictedList = new FastObservableCollection<IWordPredicted>( internalList );
            _wordPredictedCollection = new WordPredictedCollection( _predictedList );

            LoadEngine();
            TextualContextService.TextualContextChanged += OnTextualContextServiceChanged;
            Feature.PropertyChanged += OnFeaturePropertyChanged;
        }

        public virtual void Stop()
        {
            TextualContextService.TextualContextChanged -= OnTextualContextServiceChanged;
            Feature.PropertyChanged -= OnFeaturePropertyChanged;

            EngineFactory.Release( _engine );
            _engine = null;
            _predictedList.Clear();
        }

        public virtual void Teardown()
        {
        }

        private void LoadEngine()
        {
            _asyncEngineContinuation = EngineFactory.CreateAsync( Feature.Engine ).ContinueWith( task =>
            {
                if( _engine == null ) _engine = task.Result;
            } );
        }

        protected virtual void OnFeaturePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "Engine" )
            {
                EngineFactory.Release( _engine );
                _engine = null;
                LoadEngine();
            }
        }

        protected virtual void OnTextualContextServiceChanged( object sender, EventArgs e )
        {
            if( _engine == null ) _asyncEngineContinuation.ContinueWith( task => FeedPredictedList() );
            else FeedPredictedList();
        }

        private void FeedPredictedList()
        {
            if( _engine != null )
            {
                string rawContext = TextualContextService.GetTextualContext();
                PredictionLogger.Instance.Trace( "RawContext: {0}.", rawContext );

                var originTask = _engine.PredictAsync( rawContext, Feature.MaxSuggestedWords );
                originTask.ContinueWith( task =>
                {
                    PredictionLogger.Instance.Trace( "{0} items currently.", _predictedList.Count );
                    PredictionLogger.Instance.Trace( "{0}: {1}", task.Result.Count, String.Join( " ", task.Result.Select( w => w.Word ) ) );

                    _predictedList.ReplaceItems( task.Result );
                }, TaskContinuationOptions.OnlyOnRanToCompletion );
            }
        }

    }
}
