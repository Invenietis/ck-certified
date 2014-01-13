using System;
using CK.Plugin;

namespace CK.WordPredictor.Model
{
    /// <summary>
    /// A pub/sub service for successful word prediction. 
    /// A plugin that need to know if a word has been successfully predicted by a prediction engine musts subscribe to <see cref="WordPredictionSuccessful"/>.
    /// Conversely, a plugin can warn the world that a word has been successfully predicted by a prediction engine by calling <see cref="WordHasBeenChosen"/>.
    /// </summary>
    public interface IWordPredictedService : IDynamicService
    {
        /// <summary>
        /// This event is raised when a word has been chosen by the user. 
        /// We therefore consider that the prediction is successful.
        /// </summary>
        event EventHandler<WordPredictionSuccessfulEventArgs> WordPredictionSuccessful;

        /// <summary>
        /// Calls this whenever a predicted word has been chosen by the user.
        /// </summary>
        /// <param name="word">The word that was predicted by a prediction engine.</param>
        void WordHasBeenChosen( string word );
    }
}
