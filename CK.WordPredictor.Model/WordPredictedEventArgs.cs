using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.WordPredictor.Model
{
    public class WordPredictedEventArgs : EventArgs
    {
        public string Word { get; private set; }

        public WordPredictedEventArgs( string word )
        {
            Word = word;
        }
    }
}
