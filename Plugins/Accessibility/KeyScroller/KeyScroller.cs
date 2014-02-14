using System;
using System.Collections.Generic;
using System.Linq;
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
        public static List<string> AvailableStrategies = new List<string>();

        internal const string PluginIdString = "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "KeyScroller";

        Dictionary<string, IHighlightableElement> _registeredElements;
        IScrollingStrategy _scrollingStrategy;
        DispatcherTimer _timer;
        Dictionary<string, IScrollingStrategy> _strategies;
        ITrigger _currentTrigger;

        public IPluginConfigAccessor Configuration { get; set; }

        public IReadOnlyList<IHighlightableElement> RegisteredElements
        {
            get { return _registeredElements.Values.ToReadOnlyList(); }
        }

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

            _registeredElements = new Dictionary<string, IHighlightableElement>();
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
                    return new OneByOneScrollingStrategy( _timer, _registeredElements, Configuration );

                case "SplitScrollingStrategy":
                    if( _strategies.ContainsKey( name ) ) return _strategies[name];
                    return new HalfZoneScrollingStrategy( _timer, _registeredElements, Configuration );
                
                default:
                    if( _strategies.ContainsKey( "BasicScrollingStrategy" ) ) return _strategies["BasicScrollingStrategy"];
                    return new ZoneScrollingStrategy( _timer, _registeredElements, Configuration );
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

        public void RegisterTree( string nameSpace, IHighlightableElement element )
        {
            if( !_registeredElements.ContainsKey( nameSpace ) )
            {
                _registeredElements.Add( nameSpace, element );
                if( !_scrollingStrategy.IsStarted ) _scrollingStrategy.Start();
            }
        }

        public void UnregisterTree( string nameSpace, IHighlightableElement element )
        {
            IHighlightableElement value;
            if( _registeredElements.TryGetValue( nameSpace, out value ) && value == element )
            {
                _registeredElements.Remove( nameSpace );

                BrowseTree( element, e =>
                {
                    var iheus = e as IHighlightableElementController;
                    if( iheus != null ) iheus.OnUnregisterTree();
                    return false;
                } );
            }

            if( _registeredElements.Count == 0 )
            {
                _scrollingStrategy.Stop();
            }

            _scrollingStrategy.ElementUnregistered( element );
        }

        public bool RegisterInRegisteredElementAt( string nameSpace, string extensibleElementName, ChildPosition position, IHighlightableElement element )
        {
            IHighlightableElement registeredElement;
            if( _registeredElements.TryGetValue( nameSpace, out registeredElement ) )
            {
                return BrowseTree( registeredElement, e =>
                {
                    IExtensibleHighlightableElement extensibleElement = e as IExtensibleHighlightableElement;
                    if( extensibleElement != null && extensibleElement.Name == extensibleElementName ) 
                        return extensibleElement.RegisterElementAt( position, element );
                    else return false;
                } );
            }
            return false;
        }

        public bool UnregisterInRegisteredElement( string nameSpace, string extensibleElementName, ChildPosition position, IHighlightableElement element )
        {
            IHighlightableElement registeredElement;
            if( _registeredElements.TryGetValue( nameSpace, out registeredElement ) )
            {
                return BrowseTree( registeredElement, e =>
                {
                    IExtensibleHighlightableElement extensibleElement = e as IExtensibleHighlightableElement;
                    if( extensibleElement != null && extensibleElement.Name == extensibleElementName ) return extensibleElement.UnregisterElement( position, element );
                    else return false;
                } );
            }
            return false;
        }

        // if the Func return true, we leave BrowseTree
        bool BrowseTree( IHighlightableElement element, Func<IHighlightableElement,bool> doing )
        {
            if( doing( element ) ) return true;

            foreach( var child in element.Children )
            {
                if( child.Children != null && child.Children.Count > 0 )
                {
                    if( BrowseTree( child, doing ) ) return true;
                }
                if( doing( child ) ) return true;
            }
            return false;
        }

        public void Pause( bool forceEndHighlight = false )
        {
            _scrollingStrategy.Pause( forceEndHighlight );
        }

        public void Resume()
        {
            _scrollingStrategy.Resume();
        }

        //public event EventHandler<HighlightEventArgs> BeginHighlight
        //{
        //    add
        //    {
        //        foreach( var kp in _strategies )
        //        {
        //            kp.Value.BeginHighlight += value;
        //        }
        //    }
        //    remove
        //    {
        //        foreach( var kp in _strategies )
        //        {
        //            kp.Value.BeginHighlight -= value;
        //        }
        //    }
        //}

        //public event EventHandler<HighlightEventArgs> EndHighlight
        //{
        //    add
        //    {
        //        foreach( var kp in _strategies )
        //        {
        //            kp.Value.EndHighlight += value;
        //        }
        //    }
        //    remove
        //    {
        //        foreach( var kp in _strategies )
        //        {
        //            kp.Value.EndHighlight -= value;
        //        }
        //    }
        //}

        //public event EventHandler<HighlightEventArgs> SelectElement
        //{
        //    add
        //    {
        //        foreach( var kp in _strategies )
        //        {
        //            kp.Value.SelectElement += value;
        //        }
        //    }
        //    remove
        //    {
        //        foreach( var kp in _strategies )
        //        {
        //            kp.Value.SelectElement -= value;
        //        }
        //    }
        //}

        #endregion

        private void OnInputTriggered( ITrigger t )
        {
            _scrollingStrategy.OnExternalEvent();
        }
    }
}
