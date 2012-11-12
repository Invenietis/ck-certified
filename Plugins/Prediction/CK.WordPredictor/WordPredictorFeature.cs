using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    [Plugin( "{4DC42B82-4B29-4896-A548-3086AA9421D7}", PublicName = "WordPredictor Feature", Categories = new string[] { "Advanced", "Prediction" } )]
    public class WordPredictorFeature : IPlugin, IWordPredictorFeature
    {
        private IPredictionContextFactory _predictionContextFactory;

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
            get { return Config.User.TryGet( "FilterAlreadyShownWords", true ); }
        }

        public int MaxSuggestedWords
        {
            get { return Config.User.TryGet( "MaxSuggestedWords", 10 ); }
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

        public IPredictionContextFactory PredictionContextFactory
        {
            get { return _predictionContextFactory ?? (_predictionContextFactory = new DefaultPredictionContextFactory( Context, this )); }
        }

        class DefaultPredictionContextFactory : IPredictionContextFactory
        {
            readonly IWordPredictorFeature _feature;
            readonly IKeyboardContext _keyboardContext;

            public DefaultPredictionContextFactory( IKeyboardContext keyboardContext, IWordPredictorFeature feature )
            {
                _keyboardContext = keyboardContext;
                _feature = feature;
            }

            protected virtual int KeyWidth
            {
                get { return _keyboardContext.CurrentKeyboard.CurrentLayout.W / (_feature.MaxSuggestedWords + 1); }
            }

            protected virtual int KeyHeight
            {
                get { return 40; }
            }

            public IZone CreatePredictionZone( IKeyboard kb )
            {
                IZone predictionZone = kb.Zones[PredictionZoneName];
                if( predictionZone != null ) predictionZone.Destroy();

                return kb.Zones.Create( PredictionZoneName );
            }

            public IKey CreatePredictionKey( IZone zone, int index )
            {
                if( zone == null ) throw new ArgumentNullException( "zone" );

                var key= zone.Keys.Create( index );
                key.CurrentLayout.Current.Visible = false;
                CustomizePredictionKey( key );
                return key;
            }

            protected virtual void CustomizePredictionKey( IKey key )
            {
                if( key == null ) throw new ArgumentNullException( "key" );

                ILayoutKeyMode layoutKeyMode = key.CurrentLayout.Current;

                int offset = 2;
                layoutKeyMode.X = key.Index * (KeyWidth + 5) + offset;
                layoutKeyMode.Y = 5;
                layoutKeyMode.Width = KeyWidth;
                layoutKeyMode.Height = KeyHeight;
            }

            public string PredictionZoneName
            {
                get { return "Prediction"; }
            }
        }
    }
}
