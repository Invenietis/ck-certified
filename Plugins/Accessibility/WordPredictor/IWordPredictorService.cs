using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordPredictor
{
    public interface IWordPredictorService
    {
        event EventHandler<WordPredictedEventArgs> WordPredicted;

        void Predict( string word );
    }
}
