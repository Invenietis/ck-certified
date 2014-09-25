#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor.Sybille\SybilleWordPredictorEngine.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Plugin;
using CK.WordPredictor.Model;
using Sybille = WordPredictor;

namespace CK.WordPredictor.Engines
{
    public class SybilleWordPredictorEngine : IWordPredictorEngine, IDisposable
    {
        const int NB_RETRY = 5;

        Sybille.WordPredictor _sybille;
        IService<IWordPredictorFeature> _wordPredictionFeature;

        bool _constructionSuccess;
        int _currentRetryCount;

        public bool ConstructionSuccess
        {
            get { return _constructionSuccess; }
        }

        const int MaxPredictRetryCount = 2;

        public SybilleWordPredictorEngine( IService<IWordPredictorFeature> wordPredictionFeature, string languageFileName, string userLanguageFileName, string userTextsFileName )
        {
            _currentRetryCount = NB_RETRY;

            try
            {
                //Sybille can throw an exception "Loading error"
                LoadSybilleWordPredictor( languageFileName, userLanguageFileName, userTextsFileName );

                //the plugin can be stopped before the construction have finished 
                if( wordPredictionFeature.Status < InternalRunningStatus.Starting )
                {
                    _constructionSuccess = false;
                }
                else
                {
                    _sybille.FilterAlreadyShownWords = wordPredictionFeature.Service.FilterAlreadyShownWords;

                    _wordPredictionFeature = wordPredictionFeature;
                    _wordPredictionFeature.Service.PropertyChanged += OnWordPredictionFeaturePropertyChanged;
                    _constructionSuccess = true;
                }
            }
            catch( Exception e ) //Cannot instanciate the engine
            {
                _constructionSuccess = false;
            }
        }

        public SybilleWordPredictorEngine( IService<IWordPredictorFeature> wordPredictionFeature, string languageFileName, string userLanguageFileName, string userTextsFileName, string semMatrix, string semWords, string semLambdas )
        {
            _currentRetryCount = NB_RETRY;

            try
            {
                //Sybille can throw an exception "Loading error"
                LoadSybilleWordPredictor( languageFileName, userLanguageFileName, userTextsFileName, semMatrix, semWords, semLambdas );


                //the plugin can be stopped before the construction have finished 
                if( wordPredictionFeature.Status < InternalRunningStatus.Starting )
                {
                    _constructionSuccess = false;
                }
                else
                {
                    _wordPredictionFeature = wordPredictionFeature;
                    _wordPredictionFeature.Service.PropertyChanged += OnWordPredictionFeaturePropertyChanged;

                    _sybille.FilterAlreadyShownWords = _wordPredictionFeature.Service.FilterAlreadyShownWords;

                    _constructionSuccess = true;
                }
            }
            catch( Exception e ) //Cannot instanciate the engine
            {
                _constructionSuccess = false;
            }
        }

        private void LoadSybilleWordPredictor( string languageFileName, string userLanguageFileName, string userTextsFileName, string semMatrix, string semWords, string semLambdas )
        {
            //TODO log the loading error
            if( _currentRetryCount == 0 ) return;
            try
            {
                _sybille = new Sybille.WordPredictor( languageFileName, userLanguageFileName, userTextsFileName, semMatrix, semWords, semLambdas );
            }
            catch( Exception e )
            {
                _currentRetryCount--;
                LoadSybilleWordPredictor( languageFileName, userLanguageFileName, userTextsFileName, semMatrix, semWords, semLambdas );
            }
        }

        private void LoadSybilleWordPredictor( string languageFileName, string userLanguageFileName, string userTextsFileName )
        {
            //TODO log the loading error
            if( _currentRetryCount == 0 ) return;
            try
            {
                _sybille = new Sybille.WordPredictor( languageFileName, userLanguageFileName, userTextsFileName );
            }
            catch( Exception e )
            {
                _currentRetryCount--;
                LoadSybilleWordPredictor( languageFileName, userLanguageFileName, userTextsFileName );
            }
        }


        void OnWordPredictionFeaturePropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "FilterAlreadyShownWords" && _sybille != null )
            {
                _sybille.FilterAlreadyShownWords = _wordPredictionFeature.Service.FilterAlreadyShownWords;
            }
        }

        Task<ICKReadOnlyList<IWordPredicted>> _currentlyRunningTask;
        CancellationTokenSource cancellationSource = null;

        public Task<ICKReadOnlyList<IWordPredicted>> PredictAsync( string rawContext, int maxSuggestedWords )
        {
            if( _currentlyRunningTask != null && _currentlyRunningTask.Status <= TaskStatus.Running )
            {
                Debug.Assert( cancellationSource != null );
                cancellationSource.Cancel();
                //_currentlyRunningTask.Wait( cancellationSource.Token );
                cancellationSource.Dispose();
                cancellationSource = new CancellationTokenSource();
                PredictionLogger.Instance.Trace().Send( "Prediction Canceled" );
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

        public ICKReadOnlyList<IWordPredicted> Predict( string rawContext, int maxSuggestedWords )
        {
            int retryCount = MaxPredictRetryCount;
            return InternalPredict( rawContext, maxSuggestedWords, ref retryCount );
        }

        private ICKReadOnlyList<IWordPredicted> InternalPredict( string rawContext, int maxSuggestedWords, ref int retryCount )
        {
            try
            {
                var predicted = _sybille
                    .Predict( rawContext, maxSuggestedWords )
                    .Select( t => new WeightlessWordPredicted( t ) )
                    .ToReadOnlyList();

                PredictionLogger.Instance.Trace().Send( "Predicted < {0} > from < {1} >", String.Join( ", ", predicted.Select( w => w.Word ) ), rawContext.Replace( ' ', '_' ) );

                return predicted;
            }
            catch( ArgumentException ex )
            {
                PredictionLogger.Instance.Error().Send( ex.Message );
                return RetryPredict( rawContext, maxSuggestedWords, ref retryCount );
            }
            catch( IndexOutOfRangeException outOfRangeEx )
            {
                PredictionLogger.Instance.Error().Send( outOfRangeEx.Message );
                return RetryPredict( rawContext, maxSuggestedWords, ref retryCount );
            }
        }

        private ICKReadOnlyList<IWordPredicted> RetryPredict( string rawContext, int maxSuggestedWords, ref int retryCount )
        {
            if( retryCount > 0 )
            {
                retryCount--;
                PredictionLogger.Instance.Trace().Send( "Attempt n°{0}", MaxPredictRetryCount - retryCount );
                return InternalPredict( rawContext, maxSuggestedWords, ref retryCount );
            }
            return CKReadOnlyListEmpty<IWordPredicted>.Empty;
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
                    PredictionLogger.Instance.Error().Send( ex, "While saving user predictor" );
                }
            }
        }

        #endregion
    }
}
