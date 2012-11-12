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

        IKeyboardContextPredictionFactory PredictionContextFactory { get; set; }
    }
}
