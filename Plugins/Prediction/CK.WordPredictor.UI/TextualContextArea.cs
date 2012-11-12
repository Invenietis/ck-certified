using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugins.SendInput;
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
        public IService<IWordPredictorFeature> Feature { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ITextualContextService TextualContextService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ICommandTextualContextService CommandTextualContextService { get; set; }

        TextualContextAreaWindow _window;
        IKey _sendContextKey;

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            Feature.ServiceStatusChanged += OnFeatureServiceStatusChanged;
            Feature.Service.PropertyChanged += OnFeaturePropertyChanged;
            if( Feature.Service.DisplayContextEditor ) EnableEditor();
        }

        public void Stop()
        {
            if( Feature.Service.DisplayContextEditor == false ) DisableEditor();
            Feature.ServiceStatusChanged -= OnFeatureServiceStatusChanged;
            Feature.Service.PropertyChanged -= OnFeaturePropertyChanged;
        }

        public void Teardown()
        {
        }

        void OnFeaturePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "DisplayContextEditor" )
            {
                if( Feature.Service.DisplayContextEditor && (_window == null || !_window.IsVisible) ) EnableEditor();
                if( Feature.Service.DisplayContextEditor == false && (_window != null && _window.IsVisible) ) DisableEditor();
            }
        }

        void EnableEditor()
        {
            TextualContextAreaViewModel vm = new TextualContextAreaViewModel( TextualContextService, CommandTextualContextService );
            _window = new TextualContextAreaWindow( vm )
            {
                Width = 600,
                Height = 200
            };
            _window.Show();

            Context.CurrentKeyboard.Zones.ZoneCreated += OnZoneCreated;
            Context.CurrentKeyboard.Zones.ZoneDestroyed += OnZoneDestroyed;

            var zone = Context.CurrentKeyboard.Zones[InKeyboardWordPredictor.PredictionZoneName];
            CreateSendContextKeyInPredictionZone( zone );
        }

        void DisableEditor()
        {
            if( !Feature.Service.DisplayContextEditor )
            {
                DestroySendContextKey();
                if( _window != null ) _window.Close();
                Context.CurrentKeyboard.Zones.ZoneCreated -= OnZoneCreated;
            }
        }

        void CreateSendContextKeyInPredictionZone( IZone zone )
        {
            _sendContextKey = zone.Keys.Create();

            if( _sendContextKey != null )
            {
                int wordWidth = (Context.CurrentKeyboard.CurrentLayout.W) / (Feature.Service.MaxSuggestedWords + 1) - 5;
                int offset = 2;

                _sendContextKey.Current.UpLabel = "Envoyer";
                _sendContextKey.Current.OnKeyPressedCommands.Commands.Add( "sendTextualContext" );
                _sendContextKey.CurrentLayout.Current.Visible = true;

                ConfigureKey( _sendContextKey.CurrentLayout.Current, Feature.Service.MaxSuggestedWords, wordWidth, offset );
            }
        }

        protected virtual void ConfigureKey( ILayoutKeyModeCurrent layoutKeyMode, int idx, int wordWidth, int offset )
        {
            if( layoutKeyMode == null ) throw new ArgumentNullException( "layoutKeyMode" );
            layoutKeyMode.X = idx * (wordWidth + 5) + offset;
            layoutKeyMode.Y = 5;
            layoutKeyMode.Width = wordWidth;
            layoutKeyMode.Height = 45;
        }

        void DestroySendContextKey()
        {
            if( _sendContextKey != null ) _sendContextKey.Destroy();
        }

        void OnZoneCreated( object sender, ZoneEventArgs e )
        {
            if( e.Zone.Name == InKeyboardWordPredictor.PredictionZoneName )
            {
                CreateSendContextKeyInPredictionZone( e.Zone );
            }
        }

        void OnZoneDestroyed( object sender, ZoneEventArgs e )
        {
            if( e.Zone.Name == InKeyboardWordPredictor.PredictionZoneName )
            {
                DestroySendContextKey();
            }
        }

        private void OnFeatureServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == RunningStatus.Starting )
            {
                Feature.Service.PropertyChanged += OnFeaturePropertyChanged;
            }
            if( e.Current == RunningStatus.Stopping )
            {
                Feature.Service.PropertyChanged -= OnFeaturePropertyChanged;
            }
        }

    }
}
