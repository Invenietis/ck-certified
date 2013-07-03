using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;
using CK.Core;
using CommonServices.Accessibility;
using HighlightModel;

namespace KeyScroller
{
    [Strategy( TurboScrollingStrategy.StrategyName )]
    public class TurboScrollingStrategy : SimpleScrollingStrategy
    {
        const string StrategyName = "TurboScrollingStrategy";
        TimeSpan _normalInterval;
        public TimeSpan TurboInterval { get; private set; }
        bool IsTurboMode
        {
            get
            {
                return _timer.Interval == TurboInterval;
            }
        }
        public override string Name
        {
            get { return StrategyName; }
        }

        public TurboScrollingStrategy( DispatcherTimer timer, List<IHighlightableElement> elements, int turboInterval = 100 )
            : base( timer, elements )
        {
            _normalInterval = _timer.Interval;
    
            TurboInterval = new TimeSpan( 0, 0, 0, 0, turboInterval );
            _timer.Interval = TurboInterval;
        }

        public override void OnExternalEvent()
        {
            if( _currentElement != null && (!IsTurboMode || _currentElementParents.Count == 0 ) ) //Minimized
            {
                FireSelectElement( this, new HighlightEventArgs( _currentElement ) );
                _actionType = ActionType.StayOnTheSame;
                _timer.Interval = TurboInterval;
            }
            else if( IsTurboMode )
            {
                _timer.Interval = _normalInterval;
            }
        }
    }
}
