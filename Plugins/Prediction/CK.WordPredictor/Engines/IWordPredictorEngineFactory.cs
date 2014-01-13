using System.Threading.Tasks;

namespace CK.WordPredictor.Model
{
    public interface IWordPredictorEngineFactory
    {
        IWordPredictorEngine Create( string predictorName );

        Task<IWordPredictorEngine> CreateAsync( string predictorName );

        void Release( IWordPredictorEngine engine );
    }

}
