using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.WordPredictor.Model
{
    public class PredictionAreaContentEventArgs : EventArgs
    {
        public readonly string Text;

        public readonly int CaretIndex;

        public PredictionAreaContentEventArgs( string text, int caretIndex )
        {
            Text = text;
            CaretIndex = caretIndex;
        }
    }
}
