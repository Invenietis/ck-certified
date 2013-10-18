using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.WordPredictor.Model
{
    public class PredictionAreaContentEventArgs : EventArgs
    {
        public string Text { get; private set; }

        public PredictionAreaContentEventArgs( string text )
        {
            Text = text;
        }
    }
}
