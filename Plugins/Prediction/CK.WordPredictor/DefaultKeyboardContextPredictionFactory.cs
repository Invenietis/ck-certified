using System;
using System.Linq;
using CK.Keyboard.Model;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    public class AutonomousKeyboardPredictionFactory : DefaultKeyboardContextPredictionFactory, IKeyboardContextPredictionFactory
    {
        public AutonomousKeyboardPredictionFactory( IKeyboardContext keyboardContext, IWordPredictorFeature feature )
            : base( keyboardContext, feature )
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
            get { return PredictionKeyboard.CurrentLayout.H - KeyMargin * 2; }
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

        public virtual string PredictionKeyboardAndZoneName
        {
            get { return "Prediction"; }
        }

        protected virtual int KeyWidth
        {
            get
            {
                //"+1" to keep a free slot for the "Send" button.

                // width of the container container
                // minus the total space between the keys
                // divided by the number of keys
                return (_keyboardContext.CurrentKeyboard.CurrentLayout.W - _feature.MaxSuggestedWords * KeyMargin) / (_feature.MaxSuggestedWords + 1);
            }
        }

        protected virtual int KeyHeight
        {
            get { return 40; }
        }

        protected virtual int KeyOffset
        {
            get { return 10; }
        }

        /// <summary>
        /// The margin around the keys
        /// </summary>
        protected virtual int KeyMargin
        {
            get { return 5; }
        }

        public virtual IZone CreatePredictionZone( IKeyboard keyboard, int count )
        {
            if( keyboard == null ) throw new ArgumentNullException( "keyboard" );

            IZone predictionZone = keyboard.Zones[PredictionKeyboardAndZoneName];
            if( predictionZone != null )
            {
                predictionZone.Destroy();
            }

            predictionZone = keyboard.Zones.Create( PredictionKeyboardAndZoneName );

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

            IZone zone = keyboard.Zones[PredictionKeyboardAndZoneName];
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
            CustomizePredictionKey( key, index );
            return key;
        }

        /// <summary>
        /// Sets the coordinates and dimensions of a prediction key.
        /// </summary>
        /// <param name="key">The key to place & size</param>
        /// <param name="virtualIndex">The index at which the key should be put. In most cases, the virtual index is the key's index. 
        /// If some keys are in zones that are not placed linearly, you might want to handle indexes yourself.</param>
        protected virtual void CustomizePredictionKey( IKey key, int virtualIndex )
        {
            if( key == null ) throw new ArgumentNullException( "key" );

            ILayoutKeyMode layoutKeyMode = key.CurrentLayout.Current;

            layoutKeyMode.X = virtualIndex * (KeyWidth + KeyMargin) + KeyOffset;
            layoutKeyMode.Y = KeyMargin;
            layoutKeyMode.Width = KeyWidth;
            layoutKeyMode.Height = KeyHeight;

        }

        /// <summary>
        /// Sets the coordinates and dimensions of a prediction key.
        /// Automatically sets the virtualIndex to the key's index.
        /// </summary>
        /// <param name="key">the key to place & size</param>
        protected void CustomizePredictionKey( IKey key )
        {
            CustomizePredictionKey( key, key.Index );
        }
    }
}
