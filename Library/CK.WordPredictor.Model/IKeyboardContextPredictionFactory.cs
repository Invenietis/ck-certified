using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;

namespace CK.WordPredictor.Model
{
    public interface IKeyboardContextPredictionFactory
    {
        string PredictionZoneName { get; }

        /// <summary>
        /// Creates the prediction zone with the number of keys. Pass 0 to create an empty zone.
        /// This will raises the <see cref="PredictionZoneCreated"/> after the zone has been totally created and fill of keys.
        /// If the keyboard is not supported, the zone is not created. 
        /// </summary>
        /// <param name="keyboard">The keyboard in which the prediction zone must be created</param>
        /// <param name="count">The number of keys to add to the prediction zone</param>
        /// <returns>The prediction zone model <see cref="IZone"/>.</returns>
        IZone CreatePredictionZone( IKeyboard keyboard, int count );

        IKey CreatePredictionKey( IZone zone, int index );

        event EventHandler<ZoneEventArgs> PredictionZoneCreated;

        bool IsKeyboardSupported( IKeyboard keyboard );
    }
}
