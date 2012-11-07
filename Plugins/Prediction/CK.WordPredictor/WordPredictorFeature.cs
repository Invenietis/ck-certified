using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    [Plugin( "{4DC42B82-4B29-4896-A548-3086AA9421D7}", PublicName = "WordPredictor Feature", Categories = new string[] { "Advanced", "Prediction" } )]
    public class WordPredictorFeature : IPlugin, IWordPredictorFeature
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IPluginConfigAccessor Config { get; set; }

        public bool InsertSpaceAfterPredictedWord
        {
            get { return Config.User.TryGet( "InsertSpaceAfterPredictedWord", true ); }
        }

        public bool FilterAlreadyShowWords
        {
            get { return Config.User.TryGet( "FilterAlreadyShowWords", true ); }
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

    }
}
