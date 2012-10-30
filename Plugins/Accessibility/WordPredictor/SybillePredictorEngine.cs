using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Predictor.Model;
using Sybille = WordPredictor;

namespace CK.Predictor
{
    public class SybillePredictorEngine : IPredictorEngine
    {
        Sybille.WordPredictor _sybille;

        public SybillePredictorEngine( string languageFileName, string userLanguageFileName, string userTextsFileName )
        {
            _sybille = new Sybille.WordPredictor( languageFileName, userLanguageFileName, userTextsFileName );
        }

        public SybillePredictorEngine( string languageFileName, string userLanguageFileName, string userTextsFileName, string semMatrix, string semWords, string semLambdas )
        {
            _sybille = new Sybille.WordPredictor( languageFileName, userLanguageFileName, userTextsFileName, semMatrix, semWords, semLambdas );
        }

        public IEnumerable<IWordPredicted> Predict( ITextualContextService textualService, int maxSuggestedWords )
        {
            return _sybille
                .Predict( String.Join( " ", textualService.Tokens.Select( t => t.Value ).ToArray() ), maxSuggestedWords )
                .Select( t => new SimpleWordPredicted( t ) );
        }

        public bool IsWeightedPrediction
        {
            get { return false; }
        }
    }
}
