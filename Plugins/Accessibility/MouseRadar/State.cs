using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using HighlightModel;

namespace MouseRadar
{
    public class State : IHighlightableElement
    {
        SkippingBehavior _skip;

        public RadarState Current { get; private set; }

        public Action<StateContext> Action { get; private set; }

        public State( RadarState state, Action<StateContext> action )
        {
            Current = state;
            Action = action;
            _skip = SkippingBehavior.Skip;
        }

        #region IHighlightableElement Members

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get 
            {
                return CKReadOnlyListEmpty<IHighlightableElement>.Empty;
            }
        }

        public int X
        {
            get { return 0; }
        }

        public int Y
        {
            get { return 0; }
        }

        public int Width
        {
            get { return 0; }
        }

        public int Height
        {
            get { return 0; }
        }

        public SkippingBehavior Skip
        {
            get { return _skip; }
            internal set
            {
                _skip = value;
            }
        }

        #endregion
    }

    public enum RadarState
    {
        None = 0,
        Rotate,
        Translate
    }
}
