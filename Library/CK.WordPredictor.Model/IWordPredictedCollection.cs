using System.Collections.Specialized;
using CK.Core;

namespace CK.WordPredictor.Model
{
    public interface IWordPredictedCollection : ICKReadOnlyList<IWordPredicted>, INotifyCollectionChanged
    {
    }
}
