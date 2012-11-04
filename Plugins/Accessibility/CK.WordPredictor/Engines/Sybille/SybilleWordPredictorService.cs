using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.WordPredictor.Engines;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    [Plugin( "{1764F522-A9E9-40E5-B821-25E12D10DC65}", PublicName = "Sybille", Categories = new[] { "Prediction" } )]
    public class SybilleWordPredictorService : WordPredictorServiceBase
    {
        IWordPredictorEngineFactory _engineFactory;

        public SybilleWordPredictorService()
        {
            _engineFactory = new SybilleWordPredictorEngineFactory( PluginDirectoryPath );
        }

        protected override IWordPredictorEngineFactory EngineFactory
        {
            get { return _engineFactory; }
        }
    }

}
