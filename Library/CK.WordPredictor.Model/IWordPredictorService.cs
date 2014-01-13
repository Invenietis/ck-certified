using CK.Plugin;

namespace CK.WordPredictor.Model
{
    public interface IWordPredictorService : IDynamicService
    {
        /// <summary>
        /// Gets a an observable collection of <see cref="IWordPredicted"/>.
        /// <see cref="INotifyCollectionChanged"/> is raised whenever the collection change.
        /// </summary>
        IWordPredictedCollection Words { get; }
    }
}
