using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CK.Predictor.Model;

namespace CK.Predictor
{
    internal interface IPredictorEngineFactory
    {
        IPredictorEngine Create( string predictorName );

        void Release( IPredictorEngine engine );
    }

    internal class WordPredictorEngineFactory : IPredictorEngineFactory
    {
        string _resourceDirectory;
        public WordPredictorEngineFactory( string resourceDirectory )
        {
            _resourceDirectory = resourceDirectory;
        }

        public IPredictorEngine Create( string predictorName )
        {
            switch( predictorName.ToLowerInvariant() )
            {
                case "sybille": return new SybillePredictorEngine(
                    Path.Combine( _resourceDirectory, "LMbin_fr.sib" ),
                    Path.Combine( _resourceDirectory, "UserPredictor_fr.sib" ),
                    Path.Combine( _resourceDirectory, "UserTexts_fr.txt" ) );
                case "sem-sybille": return new SybillePredictorEngine(
                    Path.Combine( _resourceDirectory, "LMbin_fr.sib" ),
                    Path.Combine( _resourceDirectory, "UserPredictor_fr.sib" ),
                    Path.Combine( _resourceDirectory, "UserTexts_fr.txt" ),
                    Path.Combine( _resourceDirectory, "SemMatrix_fr.bin" ),
                    Path.Combine( _resourceDirectory, "SemWords_fr.txt" ),
                    Path.Combine( _resourceDirectory, "SemLambdas_fr.txt" ) );
                default: throw new ArgumentException( String.Format( "There is no word predictor engine for the name: {0}", predictorName ), "predictorName" );
            }
        }

        public void Release( IPredictorEngine engine )
        {
            var disposable = engine as IDisposable;
            if( disposable != null )
            {
                disposable.Dispose();
            }
        }
    }
}
