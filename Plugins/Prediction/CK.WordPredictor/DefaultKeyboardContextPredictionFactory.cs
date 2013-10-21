using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    public class AutonomousKeyboardPredictionFactory : DefaultKeyboardContextPredictionFactory, IKeyboardContextPredictionFactory
    {
        public AutonomousKeyboardPredictionFactory( IKeyboardContext keyboardContext, IWordPredictorFeature feature ):base(keyboardContext,feature)
        {
        }

        IKeyboard PredictionKeyboard
        {
            get
            {
                return KeyboardContext.Keyboards.Actives.FirstOrDefault( x => x.Name == "Prediction" );
            }
        }

        protected override int KeyHeight
        {
            get { return PredictionKeyboard.CurrentLayout.H - KeySpace*2; }
        }
    }

    public class DefaultKeyboardContextPredictionFactory : IKeyboardContextPredictionFactory
    {
        readonly IWordPredictorFeature _feature;
        readonly IKeyboardContext _keyboardContext;

        protected IWordPredictorFeature Feature
        {
            get { return _feature; }
        } 

        protected IKeyboardContext KeyboardContext
        {
            get { return _keyboardContext; }
        } 

        public event EventHandler<ZoneEventArgs>  PredictionZoneCreated;

        public DefaultKeyboardContextPredictionFactory( IKeyboardContext keyboardContext, IWordPredictorFeature feature )
        {
            _keyboardContext = keyboardContext;
            _feature = feature;
        }

        public virtual string PredictionZoneName
        {
            get { return "Prediction"; }
        }

        protected virtual int KeyWidth
        {
            get { return _keyboardContext.CurrentKeyboard.CurrentLayout.W / (_feature.MaxSuggestedWords + 1) - KeySpace; }
        }

        protected virtual int KeyHeight
        {
            get { return 40; }
        }

        protected virtual int KeyOffset
        {
            get { return 2; }
        }

        protected virtual int KeySpace
        {
            get { return 5; }
        }

        public virtual IZone CreatePredictionZone( IKeyboard keyboard, int count )
        {
            if( keyboard == null ) throw new ArgumentNullException( "keyboard" );

            IZone predictionZone = keyboard.Zones[PredictionZoneName];
            if( predictionZone != null ) predictionZone.Destroy();

            predictionZone = keyboard.Zones.Create( PredictionZoneName );

            for( int i = 0; i < count; ++i )
            {
                CreatePredictionKey( predictionZone, i );
            }

            if( PredictionZoneCreated != null )
                PredictionZoneCreated( this, new ZoneEventArgs( predictionZone ) );

            return predictionZone;
        }


        public virtual void RemovePredictionZone( IKeyboard keyboard )
        {
            if( keyboard == null ) throw new ArgumentNullException( "keyboard" );

            IZone zone = keyboard.Zones[PredictionZoneName];
            if( zone != null )
            {
                for( int i = zone.Keys.Count - 1; i >= 0; i-- )
                {
                    zone.Keys[i].Destroy();
                }

                zone.Destroy();
            }
        }

        public virtual IKey CreatePredictionKey( IZone zone, int index )
        {
            if( zone == null ) throw new ArgumentNullException( "zone" );

            var key= zone.Keys.Create( index );
            key.CurrentLayout.Current.Visible = false;
            CustomizePredictionKey( key );
            return key;
        }

        protected virtual void CustomizePredictionKey( IKey key )
        {
            if( key == null ) throw new ArgumentNullException( "key" );

            ILayoutKeyMode layoutKeyMode = key.CurrentLayout.Current;

            layoutKeyMode.X = key.Index * (KeyWidth + KeySpace) + KeyOffset;
            layoutKeyMode.Y = KeySpace;
            layoutKeyMode.Width = KeyWidth;
            layoutKeyMode.Height = KeyHeight;
        }
    }
}
