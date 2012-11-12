using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Plugin;

namespace CK.WordPredictor.Model
{
    /// <summary>
    /// This service brings together all prediction features.
    /// </summary>
    public interface IWordPredictorFeature : IDynamicService, INotifyPropertyChanged
    {
        bool FilterAlreadyShownWords { get; }

        bool InsertSpaceAfterPredictedWord { get; }

        int MaxSuggestedWords { get; }

        string Engine { get; }

        bool DisplayContextEditor { get; }

        IPredictionContextFactory PredictionContextFactory { get; }
    }

    public interface IPredictionContextFactory
    {
        string PredictionZoneName { get; }

        IZone CreatePredictionZone( IKeyboard keyboard );

        IKey CreatePredictionKey( IZone zone, int index );
    }
}
