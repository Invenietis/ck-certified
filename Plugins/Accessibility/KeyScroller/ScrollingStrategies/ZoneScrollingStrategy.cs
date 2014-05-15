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
        public const string StrategyName = "ZoneScrollingStrategy";

        public override string Name
        {
            get { return StrategyName; }
        }
    }
}
