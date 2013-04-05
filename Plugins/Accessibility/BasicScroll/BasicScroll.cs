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

namespace BasicScroll
{
    [Plugin( BasicScrollPlugin.PluginIdString,
           PublicName = PluginPublicName,
           Version = BasicScrollPlugin.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class BasicScrollPlugin : IPlugin, IHighlighterService
    {
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );
        internal const string PluginIdString = "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "BasicScroll";

        List<IHighlightableElement> _registeredElements;
        DefaultScrollingStrategy _scrollingStrategy;
        DispatcherTimer _timer;

        public IPluginConfigAccessor Configuration { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITriggerService> ExternalInput { get; set; }


        public bool Setup( IPluginSetupInfo info )
        {
            _timer = new DispatcherTimer();
            int timerSpeed = Configuration.User.GetOrSet( "Speed", 1000 );
            _timer.Interval = new TimeSpan( 0, 0, 0, 0, timerSpeed );

            _registeredElements = new List<IHighlightableElement>();
            _scrollingStrategy = new DefaultScrollingStrategy( _timer, _registeredElements );
            
            return true;
        }

        public void Start()
        {
            Configuration.ConfigChanged += ( o, e ) =>
            {
                if( e.MultiPluginId.Any( u => u.UniqueId == BasicScrollPlugin.PluginId.UniqueId ) && e.Key == "Speed" )
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
            bool getNext = true; 
            if( _registeredElements.Count == 0 )
            {
                _scrollingStrategy.Stop();
                getNext = false;
            }
            
            _scrollingStrategy.ElementUnregistered( element, getNext );
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

        private void OnExternalInputTriggered( object sender, EventArgs e )
        {
            _scrollingStrategy.OnExternalEvent();
        }

    }
}
