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

        IReadOnlyList<IWordPredicted> Predict( string rawContext, int maxSuggestedWords );

        Task<IReadOnlyList<IWordPredicted>> PredictAsync( string rawContext, int maxSuggestedWords );
    }
}
