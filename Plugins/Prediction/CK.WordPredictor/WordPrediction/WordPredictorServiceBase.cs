#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor\WordPrediction\WordPredictorServiceBase.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CK.Plugin;
using CK.WordPredictor.Model;
using CK.Core;
using System.Linq;
using System.Diagnostics;


namespace CK.WordPredictor
{
    public abstract class WordPredictorServiceBase : IPlugin, IWordPredictorService
    {
        static Func<string> _resourcePath = () => Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ) );

        FastObservableCollection<IWordPredicted> _predictedList;
        WordPredictedCollection _wordPredictedCollection;
        IWordPredictorEngine _engine;
        Task _asyncEngineContinuation;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ITextualContextService TextualContextService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IWordPredictorFeature> Feature { get; set; }

        public IWordPredictedCollection Words
        {
            get { return _wordPredictedCollection; }
        }

        protected abstract IWordPredictorEngineFactory EngineFactory { get; }

        internal protected static Func<string> PluginDirectoryPath
        {
            get { return _resourcePath; }
            set { _resourcePath = value; }
        }

        internal Task AsyncEngineContinuation
        {
            get { return _asyncEngineContinuation; }
        }

        public virtual bool Setup( IPluginSetupInfo info )
        {

            return true;
        }

        public virtual void Start()
        {
            List<IWordPredicted> internalList = new List<IWordPredicted>( Feature.Service.MaxSuggestedWords );
            _predictedList = new FastObservableCollection<IWordPredicted>( internalList );
            _wordPredictedCollection = new WordPredictedCollection( _predictedList );

            LoadEngine();
            TextualContextService.TextualContextChanged += OnTextualContextServiceChanged;
            Feature.Service.PropertyChanged += OnFeaturePropertyChanged;
        }

        public virtual void Stop()
        {
            TextualContextService.TextualContextChanged -= OnTextualContextServiceChanged;
            Feature.Service.PropertyChanged -= OnFeaturePropertyChanged;

            EngineFactory.Release( _engine );
            _engine = null;
            _predictedList.Clear();
        }

        public virtual void Teardown()
        {
        }

        private void LoadEngine()
        {
            //the engine load can be fail
            _asyncEngineContinuation = EngineFactory.CreateAsync( Feature.Service.Engine ).ContinueWith( task =>
            {
                if( _engine == null ) _engine = task.Result;
            }, TaskContinuationOptions.NotOnCanceled );
        }

        protected virtual void OnFeaturePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "Engine" )
            {
                EngineFactory.Release( _engine );
                _engine = null;
                LoadEngine();
            }
        }

        protected virtual void OnTextualContextServiceChanged( object sender, EventArgs e )
        {
            if( _engine == null ) _asyncEngineContinuation.ContinueWith( task => FeedPredictedList() );
            else FeedPredictedList();
        }

        private void FeedPredictedList()
        {
            if( _engine != null )
            {
                string rawContext = TextualContextService.GetTextualContext();
                PredictionLogger.Instance.Trace( "RawContext: {0}.", rawContext );

                var originTask = _engine.PredictAsync( rawContext, Feature.Service.MaxSuggestedWords );
                originTask.ContinueWith( task =>
                {
                    PredictionLogger.Instance.Trace( "{0} items currently.", _predictedList.Count );
                    PredictionLogger.Instance.Trace( "{0}: {1}", task.Result.Count, String.Join( " ", task.Result.Select( w => w.Word ) ) );

                    _predictedList.ReplaceItems( task.Result );
                }, TaskContinuationOptions.OnlyOnRanToCompletion );
            }
        }

    }
}
