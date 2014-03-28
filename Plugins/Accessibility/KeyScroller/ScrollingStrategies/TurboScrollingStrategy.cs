using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using CK.Plugin.Config;
using HighlightModel;
using System.Timers;

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
        double _normalInterval;
        Timer _normalSpeedTimer;

        public double TurboInterval { get; private set; }

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
            if( Johnnie.Current.IsHighlightableTreeRoot && LastDirective.NextActionType != ActionType.EnterChild )
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

        public override void Setup( Timer timer, Dictionary<string, IHighlightableElement> elements, IPluginConfigAccessor config )
        {
            base.Setup( timer, elements, config );

            _normalInterval = Timer.Interval;
            TurboInterval = (double) Configuration.User.GetOrSet( "TurboSpeed", 100 );
            _normalSpeedTimer = new Timer();
            _normalSpeedTimer.Interval = 5000;
            _normalSpeedTimer.Elapsed += ( o, e ) => SetTurboWithCheck();
            Timer.Elapsed += ( o, e ) => { if( IsTurboMode ) SetTurboWithCheck(); };
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( u => u.UniqueId == ScrollerPlugin.PluginId.UniqueId ) )
            {
                if( e.Key == "Speed" )
                {
                    var newInterval = (double) (int) e.Value;
                    if( !IsTurboMode ) _normalInterval = newInterval;
                    else
                    {
                        _normalInterval = newInterval;
                        SetTurboWithCheck();
                    }
                }
                if( e.Key == "TurboSpeed" )
                {
                    TurboInterval = (double)(int) e.Value;
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
            if( Johnnie.Current != null && (!IsTurboMode || Johnnie.Current.IsHighlightableTreeRoot) )
            {
                if( Johnnie.Current.IsHighlightableTreeRoot )
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
                Timer.Interval = _normalInterval;
                _normalSpeedTimer.Start();
            }

            //State changes of the turbo mode must be taken into account immediately
            LastDirective.ActionTime = ActionTime.Immediate;
            EnsureReactivity();
        }
    }
}
