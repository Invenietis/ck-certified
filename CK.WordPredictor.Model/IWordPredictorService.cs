using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CK.WordPredictor.Model
{
    public interface IWordPredictorService
    {
        /// <summary>
        /// Gets whether this service can produce weights for its predictions.
        /// This may change at any moment.
        /// </summary>
        bool IsWeightedPrediction { get; }

        /// <summary>
        /// 
        /// </summary>
        ReadOnlyObservableCollection<IWordPredicted> Words { get; }

        IWordPredictorEngine CurrentEngine { get; }
    }
}
