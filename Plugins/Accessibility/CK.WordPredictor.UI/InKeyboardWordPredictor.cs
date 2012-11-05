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
    [Plugin( "{1756C34D-EF4F-45DA-9224-1232E96964D2}", PublicName = "Word Prediction UI - In Keyboard", Categories = new string[] { "Prediction", "Visual" } )]
    public class InKeyboardWordPredictor : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardContext Context { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IService<IWordPredictorService> WordPredictorService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWordPredictorFeature Feature { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        public const string CompatibilityKeyboardName = "Azerty";
        public const string PredictionZoneName = "Prediction";

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            if( Context != null )
            {
                if( IsKeyboardCompatible( Context.CurrentKeyboard ) )
                    CreatePredictionZone( Context.CurrentKeyboard );

                Context.CurrentKeyboardChanged += OnCurrentKeyboardChanged;
            }

            if( WordPredictorService != null )
            {
                WordPredictorService.ServiceStatusChanged += OnWordPredictorServiceStatusChanged;
                WordPredictorService.Service.Words.CollectionChanged += OnWordPredictedCollectionChanged;
            }
        }

        public void Stop()
        {
            if( WordPredictorService != null && WordPredictorService.Service != null )
            {
                WordPredictorService.Service.Words.CollectionChanged -= OnWordPredictedCollectionChanged;
                WordPredictorService.ServiceStatusChanged -= OnWordPredictorServiceStatusChanged;
            }
            if( Context != null )
            {
                foreach( IKeyboard k in Context.Keyboards )
                {
                    IZone zone = k.Zones[PredictionZoneName];
                    if( zone != null ) zone.Destroy();
                }
            }
        }

        protected virtual bool IsKeyboardCompatible( IKeyboard keyboard )
        {
            return keyboard.Name == CompatibilityKeyboardName;
        }

        void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            if( IsKeyboardCompatible( e.Current ) )
            {
                CreatePredictionZone( e.Current );
            }
        }

        void OnWordPredictorServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == RunningStatus.Stopping )
            {
                WordPredictorService.Service.Words.CollectionChanged -= OnWordPredictedCollectionChanged;
            }
            if( e.Current == RunningStatus.Starting )
            {
                WordPredictorService.Service.Words.CollectionChanged += OnWordPredictedCollectionChanged;
            }
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
                        for( int i = 0; i < Feature.MaxSuggestedWords; ++i )
                        {
                            zone.Keys[i].CurrentLayout.Current.Visible = false;
                        }
                    }
                    else if( e.Action == NotifyCollectionChangedAction.Add )
                    {
                        int idx = e.NewStartingIndex;
                        IKey key = zone.Keys[idx];
                        if( key != null )
                        {
                            IWordPredicted wordPredicted = WordPredictorService.Service.Words[e.NewStartingIndex];
                            if( wordPredicted != null )
                            {
                                key.Current.DownLabel = wordPredicted.Word;
                                key.Current.UpLabel = wordPredicted.Word;
                                key.Current.OnKeyPressedCommands.Commands.Clear();
                                key.Current.OnKeyPressedCommands.Commands.Add( CommandFromWord( wordPredicted ) );
                                key.CurrentLayout.Current.Visible = true;
                            }
                        }
                    }
                }
            }
        }

        protected virtual string CommandFromWord( IWordPredicted wordPredicted )
        {
            return String.Format( @"{0}:{1}", "sendPredictedWord", wordPredicted.Word.ToLowerInvariant() );
        }


        public void Teardown()
        {
        }

        protected virtual void CreatePredictionZone( IKeyboard kb )
        {
            IZone predictionZone = kb.Zones[PredictionZoneName];
            if( predictionZone != null ) predictionZone.Destroy();

            predictionZone = kb.Zones.Create( PredictionZoneName );
            if( predictionZone != null )
            {
                int wordWidth = Context.CurrentKeyboard.CurrentLayout.W / Feature.MaxSuggestedWords - 5;
                int offset = 2;

                for( int i = 0; i < Feature.MaxSuggestedWords; ++i )
                {
                    IKey key = predictionZone.Keys.Create( i );
                    key.CurrentLayout.Current.Visible = false;
                    ConfigureKey( key.CurrentLayout.Current, i, wordWidth, offset );
                }
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


    }
}
