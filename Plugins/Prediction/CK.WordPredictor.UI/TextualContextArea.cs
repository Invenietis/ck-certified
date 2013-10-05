using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugins.SendInputDriver;
using CK.WindowManager.Model;
using CK.WordPredictor.Model;
using CK.WordPredictor.UI.ViewModels;

namespace CK.WordPredictor.UI
{

    [Plugin( "{69E910CC-C51B-4B80-86D3-E86B6C668C61}", PublicName = "TextualContext - Input Area", Categories = new string[] { "Prediction", "Visual" } )]
    public class TextualContextArea : IPlugin
    {
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

        void WindowManager_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                RegisterWindowManager();
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                UnregisterWindowManager();
            }
        }

        void Service_Registered( object sender, WindowElementEventArgs e )
        {
            if( e.Window.Name == "Skin" )
            {
                WindowBinder.Service.Attach( _w, e.Window );
            }
        }

        private void RegisterWindowManager()
        {
            _w = new WindowElement( WindowManager.Service, _window, "TextualContextArea" );

            WindowManager.Service.Register( _w );
            WindowManager.Service.Registered += Service_Registered;
        }

        private void UnregisterWindowManager()
        {
            WindowManager.Service.Unregister( _w );
            WindowManager.Service.Registered -= Service_Registered;

            _w.Dispose();
        }

        WindowElement _w;
        TextualContextAreaWindow _window;
        IKey _sendContextKey;

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
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

            Feature.PredictionContextFactory.PredictionZoneCreated += OnZoneCreated;
            Context.CurrentKeyboard.Zones.ZoneDestroyed += OnZoneDestroyed;

            var zone = Context.CurrentKeyboard.Zones[Feature.PredictionContextFactory.PredictionZoneName];
            CreateSendContextKeyInPredictionZone( zone );

            // Window Manager
            WindowManager.ServiceStatusChanged += WindowManager_ServiceStatusChanged;
            if( WindowManager.Status == InternalRunningStatus.Started )
            {
                RegisterWindowManager();
            }
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

        void DisableEditor()
        {
            DestroySendContextKey();
            Feature.PredictionContextFactory.PredictionZoneCreated -= OnZoneCreated;
            Context.CurrentKeyboard.Zones.ZoneDestroyed -= OnZoneDestroyed;

            if( _window != null ) _window.Close();
            if( _observersChain != null ) _observersChain.Dispose();

            UnregisterWindowManager();
            WindowManager.ServiceStatusChanged -= WindowManager_ServiceStatusChanged;
        }

        void CreateSendContextKeyInPredictionZone( IZone zone )
        {
            if( zone != null )
            {
                _sendContextKey = Feature.PredictionContextFactory.CreatePredictionKey( zone, Feature.MaxSuggestedWords );

                _sendContextKey.Current.UpLabel = "Envoyer";
                _sendContextKey.Current.OnKeyDownCommands.Commands.Add( "sendPredictionAreaContent" );
                _sendContextKey.CurrentLayout.Current.Visible = true;
            }
        }

        void DestroySendContextKey()
        {
            if( _sendContextKey != null ) _sendContextKey.Destroy();
        }

        void OnZoneCreated( object sender, ZoneEventArgs e )
        {
            CreateSendContextKeyInPredictionZone( e.Zone );
        }

        void OnZoneDestroyed( object sender, ZoneEventArgs e )
        {
            if( e.Zone.Name == Feature.PredictionContextFactory.PredictionZoneName )
            {
                DestroySendContextKey();
            }
        }

        private void OnFeatureServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Starting )
            {
                Feature.PropertyChanged += OnFeaturePropertyChanged;
            }
            if( e.Current == InternalRunningStatus.Stopping )
            {
                Feature.PropertyChanged -= OnFeaturePropertyChanged;
            }
        }

    }
}
