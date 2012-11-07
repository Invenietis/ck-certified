using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WordPredictor.Model;
using Sybille = WordPredictor;

namespace CK.WordPredictor.Engines
{
    public class SybilleWordPredictorEngine : IWordPredictorEngine, IDisposable
    {
        Sybille.WordPredictor _sybille;

        public SybilleWordPredictorEngine( string languageFileName, string userLanguageFileName, string userTextsFileName )
        {
            _sybille = new Sybille.WordPredictor( languageFileName, userLanguageFileName, userTextsFileName );
            _sybille.FilterAlreadyShownWords = false;
        }

        public SybilleWordPredictorEngine( string languageFileName, string userLanguageFileName, string userTextsFileName, string semMatrix, string semWords, string semLambdas )
        {
            _sybille = new Sybille.WordPredictor( languageFileName, userLanguageFileName, userTextsFileName, semMatrix, semWords, semLambdas );
            _sybille.FilterAlreadyShownWords = false;
        }

        public IEnumerable<IWordPredicted> Predict( ITextualContextService textualService, int maxSuggestedWords )
        {
            return _sybille
                .Predict( ObtainContext( textualService ), maxSuggestedWords )
                .Select( t => new WeightlessWordPredicted( t ) );
        }

        public string ObtainContext( ITextualContextService textualService )
        {
            string tokenPhrase =  String.Join( " ", textualService.Tokens.Take( textualService.CurrentTokenIndex ).Select( t => t.Value ) );
            tokenPhrase += " ";
            tokenPhrase += textualService.Tokens.Count >= textualService.CurrentTokenIndex ?
                (textualService.Tokens[textualService.CurrentTokenIndex].Value.Substring( 0, textualService.CaretOffset )) : String.Empty;
            return tokenPhrase;
        }


        public bool IsWeightedPrediction
        {
            get { return false; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if( _sybille != null )
            {
                //_sybille.SaveUserPredictor();
                _sybille = null; // Sybille will be correctly garbage collected as we pass the ref to null.
                //GC.Collect();
            }
        }

        #endregion
    }
}
