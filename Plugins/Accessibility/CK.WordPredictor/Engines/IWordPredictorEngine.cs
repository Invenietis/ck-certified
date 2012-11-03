using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.WordPredictor.Model
{
    public interface IWordPredictorEngine
    {
        bool IsWeightedPrediction { get; }

        IEnumerable<IWordPredicted> Predict( ITextualContextService textualService, int maxSuggestedWord );
    }
}
