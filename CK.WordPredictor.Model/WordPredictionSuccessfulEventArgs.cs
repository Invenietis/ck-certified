using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.WordPredictor.Model
{
    /// <summary>
    /// Encapsulates the predicted word that has been chosen by the user.
    /// </summary>
    public class WordPredictionSuccessfulEventArgs : EventArgs
    {
        /// <summary>
        /// The predicted word.
        /// </summary>
        public string Word { get; private set; }

        public WordPredictionSuccessfulEventArgs( string word )
        {
            Word = word;
        }
    }
}
