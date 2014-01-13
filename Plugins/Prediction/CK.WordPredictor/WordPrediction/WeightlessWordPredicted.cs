using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    public class WeightlessWordPredicted : IWordPredicted
    {
        public WeightlessWordPredicted( string w )
        {
            Word = w;
        }

        public string Word { get; private set; }

        public double Weight
        {
            get { return 0; }
        }
    }
}
