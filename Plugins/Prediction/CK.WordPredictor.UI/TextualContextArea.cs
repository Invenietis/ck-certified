#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor.UI\TextualContextArea.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.WindowManager.Model;
using CK.WordPredictor.Model;
using CK.WordPredictor.UI.ViewModels;

namespace CK.WordPredictor.UI
{

    [Plugin( "{69E910CC-C51B-4B80-86D3-E86B6C668C61}", PublicName = "TextualContext - Input Area", Categories = new string[] { "Prediction", "Visual" }, Version = "1.0" )]
    public class TextualContextArea : IPlugin
    {
        internal const string WindowName = "TextualContextArea";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardContext Context { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWordPredictorFeature Feature { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IPredictionTextAreaService PredictionTextAreaService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ICommandTextualContextService CommandTextualContextService { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        TextualContextAreaWindow _window;
        IKey _sendContextKey;

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _subscriber = new WindowManagerSubscriber( WindowManager, WindowBinder );

            Feature.PropertyChanged += OnFeaturePropertyChanged;
            PredictionTextAreaService.PredictionAreaTextSent += OnPredictionAreaContentSent;

            if( Feature.DisplayContextEditor ) EnableEditor();
        }

        public void Stop()
        {
            DisableEditor();

            Feature.PropertyChanged -= OnFeaturePropertyChanged;
            PredictionTextAreaService.PredictionAreaTextSent -= OnPredictionAreaContentSent;
        }

        public void Teardown()
        {
        }

        void OnFeaturePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "DisplayContextEditor" )
            {
                if( Feature.DisplayContextEditor ) EnableEditor();
                if( Feature.DisplayContextEditor == false ) DisableEditor();
            }
        }

        WindowManagerSubscriber _subscriber;
        TextualContextAreaViewModel _textArea;
        IDisposable _observersChain;

        void EnableEditor()
        {
            TimeSpan dueTime = TimeSpan.FromMilliseconds( 250 );

            _textArea = new TextualContextAreaViewModel( PredictionTextAreaService, CommandTextualContextService );

            var propertyChangedEvents = Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
              h => new PropertyChangedEventHandler( ( sender, e ) => h( e ) ),
              h => _textArea.PropertyChanged += h,
              h => _textArea.PropertyChanged -= h );

            var textualContextChanged = propertyChangedEvents
                .Where( x => x.PropertyName == "TextualContext" )
                .Throttle( dueTime );

            var caretIndexChanged = propertyChangedEvents
                .Where( x => x.PropertyName == "CaretIndex" )
                .Throttle( dueTime );

            var isFocusedChanged = propertyChangedEvents.Where( x => x.PropertyName == "IsFocused" );

            _observersChain = textualContextChanged
                .Merge( caretIndexChanged )
                .Merge( isFocusedChanged )
                .Subscribe( OnTextAreaPropertyChanged );

            _window = new TextualContextAreaWindow( _textArea )
            {
                Width = 600,
                Height = 200
            };
            _window.Show();

            //Feature.PredictionContextFactory.PredictionZoneCreated += OnZoneCreated;
            Feature.AutonomousKeyboardPredictionFactory.PredictionZoneCreated += OnAutonomousZoneCreated;

            CreateSendKeyInSendZone( Context.Keyboards[Feature.AutonomousKeyboardPredictionFactory.PredictionKeyboardAndZoneName] );

            EnableWindowManagerSubscription();
        }

        void DisableEditor()
        {
            DestroySendKey();

            //Feature.PredictionContextFactory.PredictionZoneCreated -= OnZoneCreated;
            Feature.AutonomousKeyboardPredictionFactory.PredictionZoneCreated -= OnAutonomousZoneCreated;

            if( _window != null ) _window.Close();
            if( _observersChain != null ) _observersChain.Dispose();
            if( _subscriber != null ) _subscriber.Unsubscribe();
        }

        /// <summary>
        /// If the skin is registered when we are launched before it, 
        /// listen to to its registration and auto-attach
        /// </summary>
        void EnableWindowManagerSubscription()
        {
            _subscriber.Subscribe( WindowName, _window );
        }

        void OnPredictionAreaContentSent( object sender, PredictionAreaContentEventArgs e )
        {
            _textArea.TextualContext = String.Empty;
        }

        void OnTextAreaPropertyChanged( PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsFocused" )
            {
                PredictionTextAreaService.IsDriven = _textArea.IsFocused;
            }
            if( e.PropertyName == "TextualContext" )
            {
                PredictionTextAreaService.ChangePredictionAreaContent( _textArea.TextualContext, _textArea.CaretIndex );
            }
            if( e.PropertyName == "CaretIndex" )
            {
                PredictionTextAreaService.ChangePredictionAreaContent( _textArea.TextualContext, _textArea.CaretIndex );
            }
        }

        void CreateSendKeyInSendZone( IKeyboard keyboard )
        {
            if( keyboard != null )
            {
                //If the prediction zone doesn't exist yet, we create it
                IZone sendZone = keyboard.Zones[Feature.AutonomousKeyboardPredictionFactory.PredictionKeyboardAndZoneName];
                if( sendZone == null )
                {
                    sendZone = keyboard.Zones.Create( Feature.AutonomousKeyboardPredictionFactory.PredictionKeyboardAndZoneName );
                }

                _sendContextKey = Feature.AutonomousKeyboardPredictionFactory.CreatePredictionKey( sendZone, Feature.MaxSuggestedWords );
                InitializeSendKey( sendZone );
            }
        }

        void InitializeSendKey( IZone predictionZone )
        {
            Debug.Assert( !_sendContextKey.CurrentLayout.Current.Visible );

            _sendContextKey.Current.UpLabel = "Envoyer";
            _sendContextKey.Current.OnKeyDownCommands.Commands.Add( "sendPredictionAreaContent" );
            _sendContextKey.CurrentLayout.Current.Visible = true;
        }

        void DestroySendKey()
        {
            if( _sendContextKey != null )
            {
                //If we destroy the key, a null reference in thrown. (the holding KeyboardVM is disposed beforehand, so the KeyDestroyed event is not handled -> the key is still bound to ConfigChanged but _zone == null)
                if( _sendContextKey.Zone != null && _sendContextKey.Context != null && _sendContextKey.Keyboard != null ) _sendContextKey.Destroy();
                _sendContextKey = null;
            }
        }

        void OnAutonomousZoneCreated( object sender, ZoneEventArgs e )
        {
            DestroySendKey();
            CreateSendKeyInSendZone( e.Zone.Keyboard );
        }

        void OnFeatureServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Starting )
            {
                Feature.PropertyChanged += OnFeaturePropertyChanged;
            }
            if( e.Current <= InternalRunningStatus.Stopping )
            {
                Feature.PropertyChanged -= OnFeaturePropertyChanged;
            }
        }
    }
}
