using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CK.Plugin;
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
            _predictedList = new ObservableCollection<IWordPredicted>();
            _wordPredictedCollection = new WordPredictedCollection( _predictedList );
            return true;
        }

        public virtual void Start()
        {
            LoadEngine();
            TextualContextService.PropertyChanged += OnTextualContextServicePropertyChanged;
            Feature.PropertyChanged += OnFeaturePropertyChanged;
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
            if( _engine != null )
            {
                _engine.PredictAsync( TextualContextService, Feature.MaxSuggestedWords ).ContinueWith( words =>
                {
                    _predictedList.Clear();
                    foreach( var w in words.Result ) _predictedList.Add( w );
                }, TaskContinuationOptions.OnlyOnRanToCompletion );
            }
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
