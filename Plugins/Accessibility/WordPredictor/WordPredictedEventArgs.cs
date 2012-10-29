using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordPredictor
{
    public class WordPredictedEventArgs : EventArgs
    {
        public string Letters { get; set; }

        public string WordPredicted { get; set; }

        public WordPredictedEventArgs( string letters, string wordPredicted )
        {
            Letters = letters;
            WordPredicted = wordPredicted;
        }
    }
}
