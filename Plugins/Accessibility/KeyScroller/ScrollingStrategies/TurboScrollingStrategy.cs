#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\KeyScroller\ScrollingStrategies\TurboScrollingStrategy.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin.Config;
using HighlightModel;

namespace Scroller
{
    /// <summary>
    /// Scrolling very fast on each key one after the other, without taking zones into account. Once a select element is triggered, the scrolling goes slowlier. 
    /// The second select element triggers the scrolled element.
    /// If the second select element is not triggered after a certain amount of time, the scrolling starts going fast again.
    /// </summary>
    [StrategyAttribute( TurboScrollingStrategy.StrategyName )]
    public class TurboScrollingStrategy : OneByOneScrollingStrategy
    {
        const string StrategyName = "TurboScrollingStrategy";
        TimeSpan _normalInterval;
        DispatcherTimer _normalSpeedTimer;

        public TimeSpan TurboInterval { get; private set; }

        bool IsTurboMode
        {
            get
            {
                return Timer.Interval == TurboInterval;
            }
        }
        
        /// <summary>
        /// Method that sets the turbo if we are not scrolling on the "root" level
        /// </summary>
        private void SetTurboWithCheck()
        {
            //If we are scrolling on a root element and that the next action is not to enter the children, we explicitely set the interval to the normal one.
            //Because we don't want to scroll too fast on the elements
            if( Walker.Current.IsHighlightableTreeRoot && LastDirective.NextActionType != ActionType.EnterChild )
                Timer.Interval = _normalInterval;
            else
            {
                Timer.Interval = TurboInterval;
                _normalSpeedTimer.Stop();
            }
        }

        public override string Name
        {
            get { return StrategyName; }
        }

        public override void Setup( DispatcherTimer timer, Func<ICKReadOnlyList<IHighlightableElement>> elements, IPluginConfigAccessor config )
        {
            base.Setup( timer, elements, config );

            _normalInterval = Timer.Interval;
            TurboInterval = new TimeSpan(0, 0, 0, 0, Configuration.User.GetOrSet( "TurboSpeed", 100 ));
            _normalSpeedTimer = new DispatcherTimer();
            _normalSpeedTimer.Interval = new TimeSpan(0, 0, 0, 5);
            _normalSpeedTimer.Tick += ( o, e ) => SetTurboWithCheck();
            Timer.Tick += ( o, e ) => { if( IsTurboMode ) SetTurboWithCheck(); };
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( u => u.UniqueId == ScrollerPlugin.PluginId.UniqueId ) )
            {
                if( e.Key == "Speed" )
                {
                    var newInterval = new TimeSpan(0, 0, 0, 0, (int) e.Value);
                    if( !IsTurboMode ) _normalInterval = newInterval;
                    else
                    {
                        _normalInterval = newInterval;
                        SetTurboWithCheck();
                    }
                }
                if( e.Key == "TurboSpeed" )
                {
                    TurboInterval = new TimeSpan( 0, 0, 0, 0, (int)e.Value );
                    if( Timer.Interval != _normalInterval ) SetTurboWithCheck();
                }
            }
        }

        public override void Start()
        {
            base.Start();
            SetTurboWithCheck();
        }

        public override void Pause( bool forceEndHighlight )
        {
            base.Pause( forceEndHighlight );
            Timer.Interval = _normalInterval;
        }

        public override void Stop()
        {
            base.Stop();
            Timer.Interval = _normalInterval;
            _normalSpeedTimer.Stop();
        }

        public override void OnExternalEvent()
        {
            // Resume, if AutoPause is actived
            if( AutoPauseActived )
            {
                DeactiveAutoPause();
                return;
            }

            if( Walker.Current != null && (!IsTurboMode || Walker.Current.IsHighlightableTreeRoot) )
            {
                if( Walker.Current.IsHighlightableTreeRoot )
                {
                    FireSelectElement();
                    SetTurboWithCheck();
                }
                else
                {
                    
                    FireSelectElement();
                    SetTurboWithCheck();
                }
            }
            else if( IsTurboMode )
            {
                Timer.Interval = _normalInterval;
                _normalSpeedTimer.Start();
            }

            //State changes of the turbo mode must be taken into account immediately
            LastDirective.ActionTime = ActionTime.Immediate;
            EnsureReactivity();
        }
    }
}
