using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.WordPredictor.Model
{
    public interface IWordPredictedService : IDynamicService
    {
        event EventHandler<WordPredictedEventArgs> WordPredicted;

        void WordHasBeenChoosen( string word );
    }
}
