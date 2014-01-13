using System.Threading.Tasks;
using CK.Core;

namespace CK.WordPredictor.Model
{
    public interface IWordPredictorEngine
    {
        bool IsWeightedPrediction { get; }

        ICKReadOnlyList<IWordPredicted> Predict( string rawContext, int maxSuggestedWords );

        Task<ICKReadOnlyList<IWordPredicted>> PredictAsync( string rawContext, int maxSuggestedWords );
    }
}
