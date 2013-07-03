using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using KeyScroller;
using CK.Core;
using CommonServices.Accessibility;
using HighlightModel;

namespace KeyScroller
{
    [Strategy( BasicScrollingStrategy.StrategyName )]
    internal class BasicScrollingStrategy : ScrollingStrategy
    {
        const string StrategyName = "BasicScrollingStrategy";

        public override string Name
        {
            get { return StrategyName; }
        }

        public BasicScrollingStrategy( DispatcherTimer timer, List<IHighlightableElement> elements ) : base(timer, elements)
        {
        }

        public override void OnExternalEvent()
        {
            if( _currentElement != null )
            {
                if( _currentElement.Children.Count > 0 ) _actionType = ActionType.EnterChild;
                else
                {
                    FireSelectElement( this, new HighlightEventArgs( _currentElement ) );
                    _actionType = ActionType.StayOnTheSame;
                }
            }
        }
    }
}
