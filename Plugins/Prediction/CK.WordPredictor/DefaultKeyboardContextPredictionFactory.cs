using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    public class DefaultKeyboardContextPredictionFactory : IKeyboardContextPredictionFactory
    {
        protected const string CompatibilityKeyboardName = "Azerty";

        readonly IWordPredictorFeature _feature;
        readonly IKeyboardContext _keyboardContext;

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

        public virtual IZone CreatePredictionZone( IKeyboard kb, int count )
        {
            if( IsKeyboardSupported( kb ) )
            {
                IZone predictionZone = kb.Zones[PredictionZoneName];
                if( predictionZone != null ) predictionZone.Destroy();

                predictionZone = kb.Zones.Create( PredictionZoneName );

                for( int i = 0; i < count; ++i )
                {
                    CreatePredictionKey( predictionZone, i );
                }

                if( PredictionZoneCreated != null )
                    PredictionZoneCreated( this, new ZoneEventArgs( predictionZone ) );

                return predictionZone;
            }
            return null;
        }

        public virtual IKey CreatePredictionKey( IZone zone, int index )
        {
            if( zone == null ) throw new ArgumentNullException( "zone" );

            var key= zone.Keys.Create( index );
            key.CurrentLayout.Current.Visible = false;
            CustomizePredictionKey( key );
            return key;
        }

        public virtual bool IsKeyboardSupported( IKeyboard keyboard )
        {
            return keyboard != null ? keyboard.Name == CompatibilityKeyboardName : false;
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
