using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Predictor.Model
{
    public interface IWordPredicted
    {
        string Word { get; }

        /// <summary>
        /// Gets the weight for this prediction.
        /// This value is guaranteed to be between 0 and 1.
        /// </summary>
        double Weight { get; }
    }
}
