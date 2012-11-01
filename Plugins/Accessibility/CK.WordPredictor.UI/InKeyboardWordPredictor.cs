using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.UI
{
    [Plugin( "{1756C34D-EF4F-45DA-9224-1232E96964D2}", PublicName = "CK.Wordpredictor.UI | InKeyboard" )]
    public class InKeyboardWordPredictor : IPlugin
    {
        [RequiredService]
        public IKeyboardContext Context { get; set; }

        [RequiredService]
        public IWordPredictorService WordPredictorService { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        public const string CompatibilityKeyboardName = "Azerty";
        public const string PredictionZoneName = "Prediction";

        public int MaxDisplayedWords
        {
            get { return Config.User.TryGet( "MaxDisplayedWords", 10 ); }
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            int keyHeight = 50;
            int keySpace = 5;
            IKeyboard kb = UpdateAzertyKeyboard( keyHeight, keySpace );
            if( kb != null )
                CreatePredictionZone( kb );

            WordPredictorService.Words.CollectionChanged += OnWordPredictedCollectionChanged;
        }

        protected void OnWordPredictedCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if( Context.CurrentKeyboard.Name == CompatibilityKeyboardName )
            {
                var zone = Context.CurrentKeyboard.Zones[PredictionZoneName];
                if( zone != null )
                {
                    if( e.Action == NotifyCollectionChangedAction.Reset )
                    {
                        zone.Destroy();
                        CreatePredictionZone( Context.CurrentKeyboard );
                    }
                    else if( e.Action == NotifyCollectionChangedAction.Add )
                    {
                        IKey key = zone.Keys.Create();
                        if( key != null )
                        {
                            int wordWidth = Context.CurrentKeyboard.CurrentLayout.W / MaxDisplayedWords - 5;
                            int offset = 2;
                            IWordPredicted wordPredicted = WordPredictorService.Words[e.NewStartingIndex];
                            key.Current.DownLabel = wordPredicted.Word;
                            key.Current.UpLabel = wordPredicted.Word;
                            //key.Current.OnKeyDownCommands.Commands.Add( CommandFromWord( wordPredicted ) );
                            key.Current.OnKeyPressedCommands.Commands.Add( CommandFromWord( wordPredicted ) );

                            key.CurrentLayout.Current.X = (e.NewStartingIndex) * 5 + (WordPredictorService.Words.Count - 1) * wordWidth + offset;
                            key.CurrentLayout.Current.Y = 5;
                            key.CurrentLayout.Current.Visible = true;
                            key.CurrentLayout.Current.Width = wordWidth;
                            key.CurrentLayout.Current.Height = 45;
                        }
                    }
                }
            }
        }

        private static string CommandFromWord( IWordPredicted wordPredicted )
        {
            return String.Format( @"sendKey""{0}""", wordPredicted.Word );
        }

        public void Stop()
        {
            int keyHeight = -50;
            int keySpace = -5;
            IKeyboard kb = UpdateAzertyKeyboard( keyHeight, keySpace );
            if( kb != null )
            {
                IZone zone = kb.Zones[PredictionZoneName];
                if( zone != null ) zone.Destroy();
            }
        }

        public void Teardown()
        {
        }


        private IKeyboard UpdateAzertyKeyboard( int keyHeight, int keySpace )
        {
            IKeyboard azertyKeyboard = Context.Keyboards[CompatibilityKeyboardName];
            if( azertyKeyboard != null )
            {
                //azertyKeyboard.CurrentLayout.LayoutZones.SelectMany( t => t.LayoutKeys ).ToList().ForEach( ( l ) =>
                //{
                //    foreach( var mode in l.LayoutKeyModes )
                //    {
                //        mode.Y = mode.Y + keyHeight + keySpace;
                //    }
                //} );
                //azertyKeyboard.CurrentLayout.H = Context.CurrentKeyboard.CurrentLayout.H + keyHeight + keySpace;
                return azertyKeyboard;
            }

            return null;
        }

        private static void CreatePredictionZone( IKeyboard kb )
        {
            if( kb.Zones[PredictionZoneName] == null )
            {
                kb.Zones.Create( PredictionZoneName );
            }
        }

    }
}
