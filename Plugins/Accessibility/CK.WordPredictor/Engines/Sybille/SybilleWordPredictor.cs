using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.WordPredictor
{
    [Plugin( "{1764F522-A9E9-40E5-B821-25E12D10DC65}", PublicName = "Sybille", Categories = new[] { "Accessibility" } )]
    public class SybilleWordPredictorService : WordPredictorServiceBase
    {
        protected override string PredictorEngine
        {
            get { return "sem-sybille"; }
        }
    }

}
