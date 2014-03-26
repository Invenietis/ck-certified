using System;
using System.IO;
using CK.Context;
using CK.Plugin;
using CK.WordPredictor.Engines;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    [Plugin( "{1764F522-A9E9-40E5-B821-25E12D10DC65}", PublicName = "Sybille", Categories = new[] { "Prediction" } , Version="1.0")]
    public class SybilleWordPredictorService : WordPredictorServiceBase
    {
        IWordPredictorEngineFactory _engineFactory;

        [RequiredService]
        public IContext Context { get; set; }

        Func<string> _userPath;

        internal Func<string> UserPath
        {
            get
            {
                if( _userPath == null )
                {
                    return () =>
                    {
                        Uri userUri = Context.ConfigManager.SystemConfiguration.CurrentUserProfile.Address;
                        
                        return Path.GetDirectoryName( userUri.LocalPath );
                    };
                }
                return _userPath;
            }
            set
            {
                _userPath = value;
            }
        }

        protected override IWordPredictorEngineFactory EngineFactory
        {
            get
            {
                return _engineFactory ?? (_engineFactory = new SybilleWordPredictorEngineFactory( PluginDirectoryPath, UserPath, Feature ));
            }
        }
    }
}
