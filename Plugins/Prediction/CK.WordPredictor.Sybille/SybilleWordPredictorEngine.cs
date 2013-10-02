using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
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

        Task<IReadOnlyList<IWordPredicted>> _currentlyRunningTask;
        CancellationTokenSource cancellationSource = null;

        public Task<IReadOnlyList<IWordPredicted>> PredictAsync( string rawContext, int maxSuggestedWords )
        {
            if( _currentlyRunningTask != null && _currentlyRunningTask.Status <= TaskStatus.Running )
            {
                Debug.Assert( cancellationSource != null );
                cancellationSource.Cancel();
                //_currentlyRunningTask.Wait( cancellationSource.Token );
                cancellationSource.Dispose();
                cancellationSource = new CancellationTokenSource();
                PredictionLogger.Instance.Trace( "Prediction Canceled" );
            }

            if( cancellationSource == null )
                cancellationSource = new CancellationTokenSource();

            _currentlyRunningTask = Task.Factory.StartNew( () =>
            {
                if( cancellationSource.IsCancellationRequested == false )
                    return Predict( rawContext, maxSuggestedWords );

                return CKReadOnlyListEmpty<IWordPredicted>.Empty;
            }, cancellationSource.Token );

            return _currentlyRunningTask;
        }

        public IReadOnlyList<IWordPredicted> Predict( string rawContext, int maxSuggestedWords )
        {
            try
            {
                var predicted = _sybille
                    .Predict( rawContext, maxSuggestedWords )
                    .Select( t => new WeightlessWordPredicted( t ) )
                    .ToArray();

                PredictionLogger.Instance.Trace( "Predicted < {0} > from < {1} >", String.Join( ", ", predicted.Select( w => w.Word ) ), rawContext.Replace( ' ', '_' ) );

                return predicted;
            }
            catch( ArgumentException ex )
            {
                PredictionLogger.Instance.Error( ex.Message );
                return CKReadOnlyListEmpty<IWordPredicted>.Empty;
            }
            catch( IndexOutOfRangeException outOfRangeEx )
            {
                PredictionLogger.Instance.Error( outOfRangeEx.Message );
                return CKReadOnlyListEmpty<IWordPredicted>.Empty;
            }
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
                try
                {
                    _sybille.SaveUserPredictor();
                    _sybille = null;
                }
                catch( Exception ex )
                {
                    PredictionLogger.Instance.Error( ex, "While saving user predictor" );
                }
            }
        }

        #endregion
    }
}
