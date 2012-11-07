using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.Engines
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
