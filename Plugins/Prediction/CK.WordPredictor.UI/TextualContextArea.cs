using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public class TextualContextArea : IPlugin, IPredictionTextAreaService
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardContext Context { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IWordPredictorFeature> Feature { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ITextualContextService TextualContextService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ICommandTextualContextService CommandTextualContextService { get; set; }

        TextualContextAreaViewModel _vm;
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


        public string Text
        {
            get
            {
                if( _vm != null )
                {
                    return _vm.TextualContext;
                }
                return String.Empty;
            }
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
            _vm = new TextualContextAreaViewModel( TextualContextService, CommandTextualContextService );
            _vm.PropertyChanged += OnTextualContextAreaPropertyChanged;

            _window = new TextualContextAreaWindow( _vm )
            {
                Width = 600,
                Height = 200
            };
            _window.Show();

            Feature.Service.PredictionContextFactory.PredictionZoneCreated += OnZoneCreated;
            Context.CurrentKeyboard.Zones.ZoneDestroyed += OnZoneDestroyed;

            var zone = Context.CurrentKeyboard.Zones[Feature.Service.PredictionContextFactory.PredictionZoneName];
            CreateSendContextKeyInPredictionZone( zone );
        }

        void OnTextualContextAreaPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( PropertyChanged != null )
                PropertyChanged( this, new PropertyChangedEventArgs( "Text" ) );
        }

        void DisableEditor()
        {
            if( !Feature.Service.DisplayContextEditor )
            {
                DestroySendContextKey();
                Feature.Service.PredictionContextFactory.PredictionZoneCreated -= OnZoneCreated;
                Context.CurrentKeyboard.Zones.ZoneDestroyed -= OnZoneDestroyed;

                _vm.PropertyChanged -= OnTextualContextAreaPropertyChanged;

                if( _window != null ) _window.Close();
            }
        }

        void CreateSendContextKeyInPredictionZone( IZone zone )
        {
            if( zone != null )
            {
                _sendContextKey = Feature.Service.PredictionContextFactory.CreatePredictionKey( zone, Feature.Service.MaxSuggestedWords + 1 );

                _sendContextKey.Current.UpLabel = "Envoyer";
                _sendContextKey.Current.OnKeyPressedCommands.Commands.Add( "sendPredictionAreaContent" );
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
            if( e.Zone.Name == Feature.Service.PredictionContextFactory.PredictionZoneName )
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
