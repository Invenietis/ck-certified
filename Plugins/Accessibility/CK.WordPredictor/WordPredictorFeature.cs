using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    [Plugin( "{4DC42B82-4B29-4896-A548-3086AA9421D7}", PublicName = "WordPredictor Feature", Categories = new string[] { "Advanced" } )]
    public class WordPredictorFeature : IPlugin, IWordPredictorFeature
    {
        public IPluginConfigAccessor Config { get; set; }

        public bool InsertSpaceAfterPredictedWord
        {
            get { return Config.User.TryGet( "InsertSpaceAfterPredictedWord", true ); }
        }

        public int MaxSuggestedWords
        {
            get { return Config.User.TryGet( "MaxSuggestedWords", 10 ); }
        }

        public string Engine
        {
            get
            {                
                return Config.User.TryGet( "Engine", "sybille" );
            }
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.Key == "Engine" )
            {

            }
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
