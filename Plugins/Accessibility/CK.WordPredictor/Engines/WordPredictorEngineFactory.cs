using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.Engines
{
    internal class WordPredictorEngineFactory : IWordPredictorEngineFactory
    {
        string _pluginResourceDirectory;
        const string _sybileDataPath = "Engines\\Sybille\\Data";

        public WordPredictorEngineFactory( string pluginResourceDirectory )
        {
            _pluginResourceDirectory = pluginResourceDirectory;
        }

        public IWordPredictorEngine Create( string predictorName )
        {
            switch( predictorName.ToLowerInvariant() )
            {
                case "sybille": return new SybilleWordPredictorEngine(
                    Path.Combine( _pluginResourceDirectory, _sybileDataPath, "LMbin_fr.sib" ),
                    Path.Combine( _pluginResourceDirectory, _sybileDataPath, "UserPredictor_fr.sib" ),
                    Path.Combine( _pluginResourceDirectory, _sybileDataPath, "UserTexts_fr.txt" ) );
                case "sem-sybille": return new SybilleWordPredictorEngine(
                    Path.Combine( _pluginResourceDirectory, _sybileDataPath, "LMbin_fr.sib" ),
                    Path.Combine( _pluginResourceDirectory, _sybileDataPath, "UserPredictor_fr.sib" ),
                    Path.Combine( _pluginResourceDirectory, _sybileDataPath, "UserTexts_fr.txt" ),
                    Path.Combine( _pluginResourceDirectory, _sybileDataPath, "SemMatrix_fr.bin" ),
                    Path.Combine( _pluginResourceDirectory, _sybileDataPath, "SemWords_fr.txt" ),
                    Path.Combine( _pluginResourceDirectory, _sybileDataPath, "SemLambdas_fr.txt" ) );
                default: throw new ArgumentException( String.Format( "There is no word predictor engine for the name: {0}", predictorName ), "predictorName" );
            }
        }

        public void Release( IWordPredictorEngine engine )
        {
            var disposable = engine as IDisposable;
            if( disposable != null )
            {
                disposable.Dispose();
            }
        }
    }
}
