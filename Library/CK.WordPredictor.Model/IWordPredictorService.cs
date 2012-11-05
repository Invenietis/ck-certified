using System;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Plugin;

namespace CK.WordPredictor.Model
{
    public interface IWordPredictorService : IDynamicService
    {
        /// <summary>
        /// Gets whether this service can produce weights for its predictions.
        /// This may change at any moment.
        /// </summary>
        bool IsWeightedPrediction { get; }

        /// <summary>
        /// 
        /// </summary>
        IWordPredictedCollection Words { get; }
    }
}
