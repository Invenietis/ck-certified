using System.Collections.Generic;
using System.Windows.Threading;
using HighlightModel;
using CK.Plugin.Config;
using System.Timers;

namespace Scroller
{
    /// <summary>
    /// Scrolling on each zone, then entering the zone to scroll on each key
    /// </summary>
    [StrategyAttribute( ZoneScrollingStrategy.StrategyName )]
    internal class ZoneScrollingStrategy : ScrollingStrategyBase
    {
        const string StrategyName = "ZoneScrollingStrategy";

        public override string Name
        {
            get { return StrategyName; }
        }

        public override void OnExternalEvent()
        {
            if( Walker.Current != null )
            {
                if( Walker.Current.Children.Count > 0 ) LastDirective.NextActionType = ActionType.EnterChild;
                else
                {
                    LastDirective.NextActionType = ActionType.StayOnTheSameOnce;
                }
                FireSelectElement();
            }
        }
    }
}
