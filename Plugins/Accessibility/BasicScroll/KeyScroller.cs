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

namespace KeyScroller
{
    [Plugin( KeyScrollerPlugin.PluginIdString,
           PublicName = PluginPublicName,
           Version = KeyScrollerPlugin.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class KeyScrollerPlugin : IPlugin, IHighlighterService
    {
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );
        public static List<string> AvalaibleStrategies = new List<string>();

        internal const string PluginIdString = "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "KeyScroller";

        List<IHighlightableElement> _registeredElements;
        IScrollingStrategy _scrollingStrategy;
        DispatcherTimer _timer;

        public IPluginConfigAccessor Configuration { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITriggerService> ExternalInput { get; set; }
        static KeyScrollerPlugin()
        {
            foreach( Strategy s in Strategy.GetStrategies() )
            {
                AvalaibleStrategies.Add( s.Name );
            }
        }
        public bool Setup( IPluginSetupInfo info )
        {
            _timer = new DispatcherTimer();
            int timerSpeed = Configuration.User.GetOrSet( "Speed", 1000 );
            _timer.Interval = new TimeSpan( 0, 0, 0, 0, timerSpeed );

            _registeredElements = new List<IHighlightableElement>();
            _scrollingStrategy = GetStrategyByName(Configuration.User.GetOrSet( "Strategy", "BasicScrollingStrategy" ));
            
            return true;
        }
        
        IScrollingStrategy GetStrategyByName(string name)
        {
            switch( name )
            {
                case "TurboScrollingStrategy":
                    return new TurboScrollingStrategy( _timer, _registeredElements, Configuration.User.GetOrSet( "TurboSpeed", 100 ) );
                  
                case "SimpleScrollingStrategy":
                    return new SimpleScrollingStrategy( _timer, _registeredElements );
                
                default:
                    return new BasicScrollingStrategy( _timer, _registeredElements );
            }
        }

        public void Start()
        {
            Configuration.ConfigChanged += ( o, e ) =>
            {
                if( e.MultiPluginId.Any( u => u.UniqueId == KeyScrollerPlugin.PluginId.UniqueId ) && e.Key == "Speed" )
                {
                    _timer.Interval = new TimeSpan( 0, 0, 0, 0, (int)e.Value );
                }
            };

            ExternalInput.Service.Triggered += OnExternalInputTriggered;
        }

        public void Stop()
        {
            ExternalInput.Service.Triggered -= OnExternalInputTriggered;

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
                _scrollingStrategy.Start();
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
            _scrollingStrategy.Start();
        }

        public event EventHandler<HighlightEventArgs> BeginHighlight
        {
            add { _scrollingStrategy.BeginHighlight += value; }
            remove { _scrollingStrategy.BeginHighlight -= value; }
        }

        public event EventHandler<HighlightEventArgs> EndHighlight
        {
            add { _scrollingStrategy.EndHighlight += value; }
            remove { _scrollingStrategy.EndHighlight -= value; }
        }

        public event EventHandler<HighlightEventArgs> SelectElement
        {
            add { _scrollingStrategy.SelectElement += value; }
            remove { _scrollingStrategy.SelectElement -= value; }
        }

        #endregion

        private void OnExternalInputTriggered( object sender, InputTriggerEventArgs e )
        {
            if( e.Source != InputSource.CiviKey )
                _scrollingStrategy.OnExternalEvent();
        }
    }
}
