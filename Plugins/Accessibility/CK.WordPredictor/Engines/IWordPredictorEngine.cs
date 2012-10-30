using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.Engines
{
    internal interface IWordPredictorEngine
    {
        bool IsWeightedPrediction { get; }

        IEnumerable<IWordPredicted> Predict( ITextualContextService textualService, int maxSuggestedWord );
    }
}
