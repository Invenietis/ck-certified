using System;
using System.ComponentModel;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    [Plugin( "{4DC42B82-4B29-4896-A548-3086AA9421D7}", PublicName = "WordPredictor Feature", Categories = new string[] { "Advanced", "Prediction" }, Version = "1.0" )]
    public class WordPredictorFeature : IPlugin, IWordPredictorFeature
    {
        private IKeyboardContextPredictionFactory _predictionContextFactory;
        private IKeyboardContextPredictionFactory _autonomousPredictionContextFactory;

        public event PropertyChangedEventHandler PropertyChanged;

        public IPluginConfigAccessor Config { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardContext Context { get; set; }

        public bool InsertSpaceAfterPredictedWord
        {
            get { return Config.User.TryGet( "InsertSpaceAfterPredictedWord", true ); }
        }

        public bool DisplayContextEditor
        {
            get { return Config.User.TryGet( "DisplayContextEditor", false ); }
        }

        public bool FilterAlreadyShownWords
        {
            get
            {
                return Config.User.TryGet( "FilterAlreadyShownWords", true );
            }
        }

        public int MaxSuggestedWords
        {
            get { return Config.User.GetOrSet( "WordPredictionMaxSuggestedWords", 5 ); }
        }

        public string Engine
        {
            get { return Config.User.TryGet( "Engine", "sem-sybille" ); }
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( PropertyChanged != null )
                PropertyChanged( this, new PropertyChangedEventArgs( e.Key ) );
        }

        public bool Setup( IPluginSetupInfo info )
        {
            Config.ConfigChanged += OnConfigChanged;
            return true;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
            Config.ConfigChanged -= OnConfigChanged;
        }

        public IKeyboardContextPredictionFactory PredictionContextFactory
        {
            get { return _predictionContextFactory ?? (_predictionContextFactory = new DefaultKeyboardContextPredictionFactory( Context, this )); }
            set { _predictionContextFactory = value; }
        }

        public IKeyboardContextPredictionFactory AutonomousKeyboardPredictionFactory
        {
            get { return _autonomousPredictionContextFactory ?? (_autonomousPredictionContextFactory = new AutonomousKeyboardPredictionFactory( Context, this )); }
            set { _autonomousPredictionContextFactory = value; }
        }

    }
}
