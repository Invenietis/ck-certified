using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.WordPredictor.Model;
using Sybille = WordPredictor;

namespace CK.WordPredictor.Engines
{
    public class SybilleWordPredictorEngine : IWordPredictorEngine, IDisposable
    {
        Sybille.WordPredictor _sybille;
        IWordPredictorFeature _wordPredictionFeature;

        public SybilleWordPredictorEngine( IWordPredictorFeature wordPredictionFeature, string languageFileName, string userLanguageFileName, string userTextsFileName )
        {
            _sybille = new Sybille.WordPredictor( languageFileName, userLanguageFileName, userTextsFileName );
            _sybille.FilterAlreadyShownWords = wordPredictionFeature.FilterAlreadyShownWords;

            _wordPredictionFeature = wordPredictionFeature;
            _wordPredictionFeature.PropertyChanged += OnWordPredictionFeaturePropertyChanged;
        }

        public SybilleWordPredictorEngine( IWordPredictorFeature wordPredictionFeature, string languageFileName, string userLanguageFileName, string userTextsFileName, string semMatrix, string semWords, string semLambdas )
        {
            _sybille = new Sybille.WordPredictor( languageFileName, userLanguageFileName, userTextsFileName, semMatrix, semWords, semLambdas );
            _sybille.FilterAlreadyShownWords = wordPredictionFeature.FilterAlreadyShownWords;

            _wordPredictionFeature = wordPredictionFeature;
            _wordPredictionFeature.PropertyChanged += OnWordPredictionFeaturePropertyChanged;
        }


        void OnWordPredictionFeaturePropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "FilterAlreadyShownWords" )
            {
                _sybille.FilterAlreadyShownWords = _wordPredictionFeature.FilterAlreadyShownWords;
            }
        }

        public Task<IEnumerable<IWordPredicted>> PredictAsync( ITextualContextService textualContext, int maxSuggestedWords )
        {
            return Task.Factory.StartNew( () =>
            {
                return Predict( textualContext, maxSuggestedWords );
            } );
        }

        public IEnumerable<IWordPredicted> Predict( ITextualContextService textualService, int maxSuggestedWords )
        {
            //This call can sometimes raise an ArgumentException
            //TODO : log it and catch it to go on.
            IEnumerable<WeightlessWordPredicted> result = new List<WeightlessWordPredicted>();
            try
            {
                result = _sybille
                    .Predict( ObtainContext( textualService ), maxSuggestedWords )
                    .Select( t => new WeightlessWordPredicted( t ) );
            }
            catch( ArgumentException e )
            {
            }
            return result;
        }

        public string ObtainContext( ITextualContextService textualService )
        {
            if( textualService.Tokens.Count > 1 )
            {
                string tokenPhrase = String.Join( " ", textualService.Tokens.Take( textualService.CurrentTokenIndex ).Select( t => t.Value ) );
                tokenPhrase += " ";
                if( textualService.Tokens.Count >= textualService.CurrentTokenIndex )
                {
                    string value = textualService.Tokens[textualService.CurrentTokenIndex].Value;
                    if( value.Length >= textualService.CaretOffset )
                    {
                        tokenPhrase += (value.Substring( 0, textualService.CaretOffset ));
                    }
                }

                return tokenPhrase;
            }
            if( textualService.Tokens.Count == 1 )
            {
                return textualService.CurrentToken.Value.Substring( 0, textualService.CaretOffset );
            }
            return String.Empty;
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
                _sybille.SaveUserPredictor();
                _sybille = null;
            }
        }

        #endregion
    }
}
