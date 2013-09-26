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

        /// <summary>
        /// Returns an enumeration of <see cref="IWordPredicted"/> from 
        /// </summary>
        /// <param name="textualContext"></param>
        /// <param name="maxSuggestedWords"></param>
        /// <returns></returns>
        IEnumerable<IWordPredicted> Predict( ITextualContextService textualContext, int maxSuggestedWords );

        Task<IEnumerable<IWordPredicted>> PredictAsync( ITextualContextService textualContext, int maxSuggestedWords );
    }
}
