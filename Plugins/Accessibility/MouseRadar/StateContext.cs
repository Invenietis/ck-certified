using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MouseRadar
{
    public class StateContext
    {
        RootElement _root;

        public StateContext(RootElement root)
        {
            _root = root;
        }

        public dynamic Data { get; private set; }

        public void GoToState( RadarState rState, dynamic data)
        {
            foreach( State s in _root.Children)
            {
                if( s.Current == rState )
                {
                    s.Skip = HighlightModel.SkippingBehavior.None;
                }
                else
                {
                    if( s.Skip != HighlightModel.SkippingBehavior.Skip ) s.Skip = HighlightModel.SkippingBehavior.Skip;
                }
            }
        }
    }
}
