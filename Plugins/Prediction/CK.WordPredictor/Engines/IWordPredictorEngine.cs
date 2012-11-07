using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.WordPredictor.Model
{
    public interface IWordPredictorEngine
    {
        bool IsWeightedPrediction { get; }

        IEnumerable<IWordPredicted> Predict( ITextualContextService textualContext, int maxSuggestedWords );

        Task<IEnumerable<IWordPredicted>> PredictAsync( ITextualContextService textualContext, int maxSuggestedWords );
    }
}
