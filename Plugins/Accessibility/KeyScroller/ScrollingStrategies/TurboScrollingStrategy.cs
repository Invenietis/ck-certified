using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using CK.Plugin.Config;
using HighlightModel;

namespace KeyScroller
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
                return _timer.Interval == TurboInterval;
            }
        }
        public override string Name
        {
            get { return StrategyName; }
        }

        public TurboScrollingStrategy( DispatcherTimer timer, List<IHighlightableElement> elements, IPluginConfigAccessor configuration )
            : base( timer, elements, configuration )
        {
            _normalInterval = _timer.Interval;
            TurboInterval = new TimeSpan( 0, 0, 0, 0, _configuration.User.GetOrSet( "TurboSpeed", 100 ) );
            _normalSpeedTimer = new DispatcherTimer( DispatcherPriority.Normal );
            _normalSpeedTimer.Interval = new TimeSpan( 0, 0, 0, 0, 5000 );
            _normalSpeedTimer.Tick += ( o, e ) => SetTurboWithCheck();
            _timer.Tick += ( o, e ) => { if( IsTurboMode ) SetTurboWithCheck(); };
        }

        /// <summary>
        /// Method that sets the turbo if we are not scrolling on the "root" level
        /// </summary>
        private void SetTurboWithCheck()
        {
            //If we are scrolling on a root element and that the next action is not to enter the children, we explicitely set the interval to the normal one.
            //Because we don't want to scroll too fast on the elements
            if( _currentElement == null || ( _currentElement.IsHighlightableTreeRoot && _lastDirective.NextActionType != ActionType.EnterChild ) )
                _timer.Interval = _normalInterval;
            else
            {
                _timer.Interval = TurboInterval;
                _normalSpeedTimer.Stop();
            }
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( u => u.UniqueId == KeyScrollerPlugin.PluginId.UniqueId ) )
            {
                if( e.Key == "Speed" )
                {
                    var newInterval = new TimeSpan( 0, 0, 0, 0, (int)e.Value );
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
                    if( _timer.Interval != _normalInterval ) SetTurboWithCheck();
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
            _timer.Interval = _normalInterval;
        }
        public override void Stop()
        {
            base.Stop();
            _timer.Interval = _normalInterval;
            _normalSpeedTimer.Stop();
        }
        public override void OnExternalEvent()
        {
            if( _currentElement != null && ( !IsTurboMode || _currentElement.IsHighlightableTreeRoot ) )
            {
                if( _currentElement.IsHighlightableTreeRoot )
                {
                    FireSelectElement();
                    SetTurboWithCheck();
                }
                else
                {
                    SetTurboWithCheck();
                    FireSelectElement();
                }
            }
            else if( IsTurboMode )
            {
                _timer.Interval = _normalInterval;
                _normalSpeedTimer.Start();
            }

            //State changes of the turbo mode must be taken into account immediately
            _lastDirective.ActionTime = ActionTime.Immediate;
            EnsureReactivity();
        }
    }
}
