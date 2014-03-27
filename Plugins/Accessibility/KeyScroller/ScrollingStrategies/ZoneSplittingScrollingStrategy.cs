using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin.Config;
using HighlightModel;
using SimpleSkin.ViewModels;
using System.Timers;

namespace KeyScroller
{
    [Strategy( HalfZoneScrollingStrategy.StrategyName )]
    internal class HalfZoneScrollingStrategy : ZoneScrollingStrategy
    {
        const string StrategyName = "HalfZoneScrollingStrategy";
        const int ChildrenLimitBeforeSplit = 6;

        public override string Name
        {
            get { return StrategyName; }
        }
    }
}
