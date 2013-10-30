using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;
using CommonServices.Accessibility;
using HighlightModel;
using InputTrigger;

namespace KeyScroller
{
    [Plugin( KeyScrollerPlugin.PluginIdString,
           PublicName = PluginPublicName,
           Version = KeyScrollerPlugin.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class KeyScrollerPlugin : IPlugin, IHighlighterService
    {
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );
        public static List<string> AvailableStrategies = new List<string>();

        internal const string PluginIdString = "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "KeyScroller";

        List<IHighlightableElement> _registeredElements;
        IScrollingStrategy _scrollingStrategy;
        DispatcherTimer _timer;
        Dictionary<string, IScrollingStrategy> _strategies;
        ITrigger _currentTrigger;

        public IPluginConfigAccessor Configuration { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITriggerService> InputTrigger { get; set; }

        //List the avalaible strategy at the class init
        static KeyScrollerPlugin()
        {
            AvailableStrategies.AddRange( StrategyAttribute.GetStrategies() );
        }

        public bool Setup( IPluginSetupInfo info )
        {
            _timer = new DispatcherTimer();
            int timerSpeed = Configuration.User.GetOrSet( "Speed", 1000 );
            _timer.Interval = new TimeSpan( 0, 0, 0, 0, timerSpeed );

            _registeredElements = new List<IHighlightableElement>();
            _strategies = new Dictionary<string, IScrollingStrategy>();

            foreach( string name in AvailableStrategies )
            {
                _strategies.Add( name, GetStrategyByName( name ) );
            }
            _scrollingStrategy = GetStrategyByName( Configuration.User.GetOrSet( "Strategy", "BasicScrollingStrategy" ) );

            return true;
        }

        IScrollingStrategy GetStrategyByName( string name )
        {
            switch( name )
            {
                case "TurboScrollingStrategy":
                    if( _strategies.ContainsKey( name ) ) return _strategies[name];
                    return new TurboScrollingStrategy( _timer, _registeredElements, Configuration );

                case "SimpleScrollingStrategy":
                    if( _strategies.ContainsKey( name ) ) return _strategies[name];
                    return new SimpleScrollingStrategy( _timer, _registeredElements, Configuration );
                case "StateStrategy":
                    if( _strategies.ContainsKey( name ) ) return _strategies[name];
                    return new ActionStrategy( _timer, _registeredElements, Configuration );

                default:
                    if( _strategies.ContainsKey( "BasicScrollingStrategy" ) ) return _strategies["BasicScrollingStrategy"];
                    return new BasicScrollingStrategy( _timer, _registeredElements, Configuration );
            }
        }

        public void Start()
        {
            Configuration.ConfigChanged += ( o, e ) =>
            {
                if( e.MultiPluginId.Any( u => u.UniqueId == KeyScrollerPlugin.PluginId.UniqueId ) )
                {
                    if( e.Key == "Strategy" )
                    {
                        _scrollingStrategy.Stop();
                        _scrollingStrategy = GetStrategyByName( e.Value.ToString() );
                        _scrollingStrategy.Start();
                    }
                    if( e.Key == "Trigger" )
                    {
                        if( _currentTrigger != null )
                        {
                            InputTrigger.Service.Unregister( _currentTrigger, OnInputTriggered );
                            _currentTrigger = Configuration.User.GetOrSet( "Trigger", InputTrigger.Service.DefaultTrigger );
                            InputTrigger.Service.RegisterFor( _currentTrigger, OnInputTriggered );
                        }
                    }
                }
            };

            _currentTrigger = Configuration.User.GetOrSet( "Trigger", InputTrigger.Service.DefaultTrigger );
            InputTrigger.Service.RegisterFor( _currentTrigger, OnInputTriggered );
        }

        public void Stop()
        {
            InputTrigger.Service.Unregister( _currentTrigger, OnInputTriggered );
            _scrollingStrategy.Stop();
        }

        public void Teardown()
        {
        }

        #region Service implementation

        public bool IsHighlighting { get { return _timer.IsEnabled; } }

        public void RegisterTree( IHighlightableElement element )
        {
            if( !_registeredElements.Contains( element ) )
            {
                _registeredElements.Add( element );
                if( !_scrollingStrategy.IsStarted ) _scrollingStrategy.Start();
            }
        }

        public void UnregisterTree( IHighlightableElement element )
        {
            _registeredElements.Remove( element );

            if( _registeredElements.Count == 0 )
            {
                _scrollingStrategy.Stop();
            }

            _scrollingStrategy.ElementUnregistered( element );
        }

        public void Pause( bool forceEndHighlight = false )
        {
            _scrollingStrategy.Pause( forceEndHighlight );
        }

        public void Resume()
        {
            _scrollingStrategy.Resume();
        }

        public event EventHandler<HighlightEventArgs> BeginHighlight
        {
            add
            {
                foreach( var kp in _strategies )
                {
                    kp.Value.BeginHighlight += value;
                }
            }
            remove
            {
                foreach( var kp in _strategies )
                {
                    kp.Value.BeginHighlight -= value;
                }
            }
        }

        public event EventHandler<HighlightEventArgs> EndHighlight
        {
            add
            {
                foreach( var kp in _strategies )
                {
                    kp.Value.EndHighlight += value;
                }
            }
            remove
            {
                foreach( var kp in _strategies )
                {
                    kp.Value.EndHighlight -= value;
                }
            }
        }

        public event EventHandler<HighlightEventArgs> SelectElement
        {
            add
            {
                foreach( var kp in _strategies )
                {
                    kp.Value.SelectElement += value;
                }
            }
            remove
            {
                foreach( var kp in _strategies )
                {
                    kp.Value.SelectElement -= value;
                }
            }
        }

        #endregion

        private void OnInputTriggered( ITrigger t )
        {
            _scrollingStrategy.OnExternalEvent();
            Console.WriteLine( "Triggered : " + t.KeyCode );
        }
    }
}
