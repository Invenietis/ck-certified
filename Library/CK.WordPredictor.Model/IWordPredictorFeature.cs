using System.ComponentModel;
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

        IKeyboardContextPredictionFactory AutonomousKeyboardPredictionFactory { get; set; }

    }
}
