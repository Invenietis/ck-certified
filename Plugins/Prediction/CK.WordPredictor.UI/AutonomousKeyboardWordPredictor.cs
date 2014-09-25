#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor.UI\AutonomousKeyboardWordPredictor.cs) is part of CiviKey. 
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.UI
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion, Categories = new string[] { "Prediction", "Visual" } )]
    public class AutonomousKeyboardWordPredictor : IPlugin
    {
        #region Plugin description

        const string PluginGuidString = "{1756C34D-EF4F-45DA-9224-1232E96964D2}";
        const string PluginVersion = "1.0.0";
        const string PluginPublicName = "Word Prediction - Autonomous Keyboard";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardContext Context { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IWordPredictorService> WordPredictorService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWordPredictorFeature Feature { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        public IKeyboard PredictionKeyboard { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( Context != null )
            {
                //Feature.PredictionContextFactory.CreatePredictionZone( Context.CurrentKeyboard, Feature.MaxSuggestedWords );
                EnsurePredictionKeyboard();

                Context.CurrentKeyboardChanged += OnCurrentKeyboardChanged;
            }

            if( WordPredictorService != null )
            {
                WordPredictorService.ServiceStatusChanged += OnWordPredictorServiceStatusChanged;
                WordPredictorService.Service.Words.CollectionChanged += OnWordPredictedCollectionChanged;
            }

            if( Feature != null )
            {
                Feature.PropertyChanged += OnFeaturePropertyChanged;
            }
        }

        void OnFeaturePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( e.PropertyName == "WordPredictionMaxSuggestedWords" )
            {
                object newValue = Config.User["WordPredictionMaxSuggestedWords"];
                if( newValue != null )
                {
                    int newIntValue = (int)newValue;
                    if( newIntValue == Feature.MaxSuggestedWords )
                        return; //We don't remove and recreate the zone if the new value equals the previous one
                }

                //Feature.PredictionContextFactory.RemovePredictionZone( Context.CurrentKeyboard );
                //Feature.PredictionContextFactory.CreatePredictionZone( Context.CurrentKeyboard, Feature.MaxSuggestedWords );
                EnsurePredictionKeyboard();
            }
        }

        //Create a Prediction Keyboard in a new window
        private void EnsurePredictionKeyboard()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( PredictionKeyboard == null )
            {
                // Also Create an new keyboard and makes it active
                PredictionKeyboard = Context.Keyboards[Feature.AutonomousKeyboardPredictionFactory.PredictionKeyboardAndZoneName];
                if( PredictionKeyboard == null )
                {
                    PredictionKeyboard = Context.Keyboards.Create( Feature.AutonomousKeyboardPredictionFactory.PredictionKeyboardAndZoneName );
                }
            }
            PredictionKeyboard.CurrentLayout.H = 50;
            PredictionKeyboard.CurrentLayout.W = 815;
            PredictionKeyboard.IsActive = true;

            Feature.AutonomousKeyboardPredictionFactory.RemovePredictionZone( PredictionKeyboard );
            Feature.AutonomousKeyboardPredictionFactory.CreatePredictionZone( PredictionKeyboard, Feature.MaxSuggestedWords );
        }

        public void Stop()
        {
            if( WordPredictorService.Status.IsStartingOrStarted )
            {
                WordPredictorService.Service.Words.CollectionChanged -= OnWordPredictedCollectionChanged;
                WordPredictorService.ServiceStatusChanged -= OnWordPredictorServiceStatusChanged;
            }
            if( Context != null )
            {
                Feature.PredictionContextFactory.RemovePredictionZone( Context.Keyboards[Feature.AutonomousKeyboardPredictionFactory.PredictionKeyboardAndZoneName] );
                Context.CurrentKeyboardChanged -= OnCurrentKeyboardChanged;
            }
            if( PredictionKeyboard != null )
            {
                PredictionKeyboard.IsActive = false;
                PredictionKeyboard.Destroy();
                PredictionKeyboard = null;
            }
        }

        void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            //Feature.PredictionContextFactory.RemovePredictionZone( e.Previous );
            //Feature.PredictionContextFactory.CreatePredictionZone( e.Current, Feature.MaxSuggestedWords );
        }

        void OnWordPredictorServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( e.Current.IsStoppingOrStopped )
            {
                WordPredictorService.Service.Words.CollectionChanged -= OnWordPredictedCollectionChanged;
            }
            else if( e.Current.IsStartingOrStarted )
            {
                WordPredictorService.Service.Words.CollectionChanged += OnWordPredictedCollectionChanged;
            }
        }

        protected void OnWordPredictedCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher != NoFocusManager.Default.ExternalDispatcher && Dispatcher.CurrentDispatcher != NoFocusManager.Default.NoFocusDispatcher, "The predicted words are supposed to be fetched by a worker thread." );

            //Manipulation of the Context is done through the main thread.
            NoFocusManager.Default.ExternalDispatcher.BeginInvoke( (Action)(() =>
            {
                var zone = Context.CurrentKeyboard.Zones[Feature.PredictionContextFactory.PredictionKeyboardAndZoneName];
                //If there is a prediction zone in the current keyboard, that's the one we are going ot fill
                if( zone != null )
                    SetupPredictionZone( e, zone );
                else if( PredictionKeyboard != null ) //otherwise we check whether we have a prediction keyboard
                    SetupPredictionZone( e, PredictionKeyboard.Zones[Feature.PredictionContextFactory.PredictionKeyboardAndZoneName] );
            }) );
        }

        private void SetupPredictionZone( NotifyCollectionChangedEventArgs e, IZone zone )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( zone != null && e.Action == NotifyCollectionChangedAction.Reset )
            {
                for( int i = 0; i < Feature.MaxSuggestedWords; ++i )
                {
                    IKey key = zone.Keys[i];
                    if( key != null )
                    {
                        key.CurrentLayout.Current.Visible = false;

                        if( i < Feature.MaxSuggestedWords && WordPredictorService.Service.Words.Count > i )
                        {
                            IWordPredicted wordPredicted = WordPredictorService.Service.Words[i];
                            if( wordPredicted != null )
                            {
                                key.Current.DownLabel = wordPredicted.Word;
                                key.Current.UpLabel = wordPredicted.Word;
                                key.Current.OnKeyDownCommands.Commands.Clear();
                                key.Current.OnKeyDownCommands.Commands.Add( CommandFromWord( wordPredicted ) );
                                key.CurrentLayout.Current.Visible = true;
                            }
                        }
                    }
                }
            }
        }

        protected virtual string CommandFromWord( IWordPredicted wordPredicted )
        {
            return String.Format( @"{0}:{1}", "sendPredictedWord", wordPredicted.Word );
        }

        public void Teardown()
        {
        }
    }
}
