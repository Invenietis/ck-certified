using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Predictor.Model
{
    internal interface IPredictorEngine
    {
        bool IsWeightedPrediction { get; }

        IEnumerable<IWordPredicted> Predict( ITextualContextService textualService, int maxSuggestedWord );
    }
}
