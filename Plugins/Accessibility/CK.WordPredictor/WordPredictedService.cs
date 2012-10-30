using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WordPredictor.Model;
using CK.Plugin;

namespace CK.WordPredictor
{
    [Plugin( "{669622D4-4E7E-4CCE-96B1-6189DC5CD5D6}", PublicName = "WordPredictedService", Categories = new string[] { "Advanced" } )]
    public class WordPredictedService : IWordPredictedService
    {
        public event EventHandler<WordPredictionSuccessfulEventArgs> WordPredictionSuccessful;

        public void WordHasBeenChosen( string word )
        {
            if( WordPredictionSuccessful != null )
                WordPredictionSuccessful( this, new WordPredictionSuccessfulEventArgs( word ) );
        }
    }
}
