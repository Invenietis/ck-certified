using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin;
using CommonServices.Accessibility;
using HighlightModel;

namespace BasicScroll
{
    [Plugin( BasicScroll.PluginIdString,
           PublicName = PluginPublicName,
           Version = BasicScroll.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class BasicScroll : IPlugin, IHighlighterService
    {
        const string PluginIdString = "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "BasicScroll";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        List<IHighlightableElement> _registeredElements;
        DefaultScrollingStrategy _scrollingStrategy;

        public bool Setup( IPluginSetupInfo info )
        {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan( 0, 0, 0, 0, 1500 );

            _registeredElements = new List<IHighlightableElement>();
            _scrollingStrategy = new DefaultScrollingStrategy( timer, _registeredElements );
            
            return true;
        }

        public void Start()
        {
        }

        public void Stop()
        {
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
            if( _registeredElements.Count == 0 ) _scrollingStrategy.Stop();
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

        #endregion

    }
}
