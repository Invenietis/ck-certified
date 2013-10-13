using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin.Config;
using CommonServices.Accessibility;
using HighlightModel;

namespace KeyScroller
{
    [StrategyAttribute( TurboScrollingStrategy.StrategyName )]
    public class TurboScrollingStrategy : SimpleScrollingStrategy
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
            _normalSpeedTimer.Tick += ( o, e ) => SetNormalIntervalTimerInterval();
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
                        _timer.Interval = TurboInterval;
                    }
                }
                if( e.Key == "TurboSpeed" )
                {
                    TurboInterval = new TimeSpan( 0, 0, 0, 0, (int)e.Value );
                    if( _timer.Interval != _normalInterval ) _timer.Interval = TurboInterval;
                }
            }
        }

        void SetNormalIntervalTimerInterval()
        {
            _timer.Interval = TurboInterval;
            _normalSpeedTimer.Stop();
        }

        public override void Start()
        {
            base.Start();
            _timer.Interval = TurboInterval;
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
            if( _currentElement != null && ( !IsTurboMode || _currentElementParents.Count == 0 ) ) //Minimized
            {
                FireSelectElement( this, new HighlightEventArgs( _currentElement ) );
                _actionType = ActionType.StayOnTheSame;
                _timer.Interval = TurboInterval;
            }
            else if( IsTurboMode )
            {
                _normalSpeedTimer.Start();
                _timer.Interval = _normalInterval;
            }
        }
    }
}
