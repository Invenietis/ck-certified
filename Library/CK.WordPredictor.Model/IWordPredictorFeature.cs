using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.WordPredictor.Model
{
    public interface IWordPredictorFeature : IDynamicService
    {
        bool InsertSpaceAfterPredictedWord { get; }

        int MaxSuggestedWords { get; }

        string Engine { get; }
    }
}
