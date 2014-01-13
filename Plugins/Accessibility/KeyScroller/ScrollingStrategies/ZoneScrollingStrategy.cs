﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using KeyScroller;
using CK.Core;
using CommonServices.Accessibility;
using HighlightModel;
using CK.Plugin.Config;

namespace KeyScroller
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

        public ZoneScrollingStrategy( DispatcherTimer timer, List<IHighlightableElement> elements, IPluginConfigAccessor configuration )
            : base( timer, elements, configuration )
        {
        }

        public override void OnExternalEvent()
        {
            if( _currentElement != null )
            {
                if( _currentElement.Children.Count > 0 ) _lastDirective.NextActionType = ActionType.EnterChild;
                else
                {
                    _lastDirective.NextActionType = ActionType.StayOnTheSameOnce;
                }
                FireSelectElement();
            }
        }
    }
}