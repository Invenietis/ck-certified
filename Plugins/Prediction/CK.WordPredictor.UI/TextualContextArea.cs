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
        public IWordPredictorFeature Feature { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ITextualContextService TextualContextService { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            TextualContextAreaViewModel vm = new TextualContextAreaViewModel( TextualContextService );
            TextualContextSmartAreaWindow window = new TextualContextSmartAreaWindow( vm )
            {
                Width = 600,
                Height = 200
            };
            window.Show();

            int wordWidth = (Context.CurrentKeyboard.CurrentLayout.W) / (Feature.MaxSuggestedWords + 1) - 5;
            int offset = 2;

            var zone = Context.CurrentKeyboard.Zones[InKeyboardWordPredictor.PredictionZoneName];
            var sendContextKey = zone.Keys.Create();
            if( sendContextKey != null )
            {
                sendContextKey.Current.UpLabel = "Envoyer";
                sendContextKey.Current.OnKeyPressedCommands.Commands.Add( "sendTextualContext" );
                sendContextKey.CurrentLayout.Current.Visible = true;
                ConfigureKey( sendContextKey.CurrentLayout.Current, Feature.MaxSuggestedWords, wordWidth, offset );
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

        public void Stop()
        {
        }

        public void Teardown()
        {
        }

    }
}
