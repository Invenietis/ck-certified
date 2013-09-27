using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    public class WeightlessWordPredicted : IWordPredicted
    {
        public WeightlessWordPredicted( IActivityLogger logger, string w )
        {
            Word = w;
            logger.Info( w );
        }

        public string Word { get; private set; }

        public double Weight
        {
            get { return 0; }
        }
    }
}
