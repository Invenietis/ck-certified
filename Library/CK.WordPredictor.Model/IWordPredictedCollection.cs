using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.WordPredictor.Model
{
    public interface IWordPredictedCollection : ICKReadOnlyList<IWordPredicted>, INotifyCollectionChanged
    {
    }
}
