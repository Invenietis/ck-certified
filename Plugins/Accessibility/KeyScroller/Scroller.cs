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
using System.Timers;

namespace KeyScroller
{
    [Plugin( ScrollerPlugin.PluginIdString,
           PublicName = PluginPublicName,
           Version = ScrollerPlugin.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class ScrollerPlugin : IPlugin, IHighlighterService
    {
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );
        public static List<string> AvailableStrategies = new List<string>();

        internal const string PluginIdString = "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Scroller";

        Dictionary<string, IHighlightableElement> _registeredElements;
        ScrollingStrategy _scrollingStrategy;
        Timer _timer;
        Dictionary<string, IScrollingStrategy> _strategies;
        ITrigger _currentTrigger;

        public IPluginConfigAccessor Configuration { get; set; }

        public IReadOnlyList<IHighlightableElement> RegisteredElements
        {
            get { return _registeredElements.Values.ToReadOnlyList(); }
        }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITriggerService> InputTrigger { get; set; }

        //List the available strategy at the class init
        static ScrollerPlugin()
        {
            AvailableStrategies.AddRange( StrategyAttribute.GetStrategies() );
        }

        public bool Setup( IPluginSetupInfo info )
        {
            _timer = new Timer();
            int timerSpeed = Configuration.User.GetOrSet( "Speed", 1000 );
            _timer.Interval = timerSpeed;

            _registeredElements = new Dictionary<string, IHighlightableElement>();
            _strategies = new Dictionary<string, IScrollingStrategy>();

            foreach ( string name in AvailableStrategies )
            {
                _strategies.Add( name, GetStrategyByName( name ) );
            }
            //_scrollingStrategy = GetStrategyByName( Configuration.User.GetOrSet( "Strategy", "BasicScrollingStrategy" ) );
            _scrollingStrategy = new ScrollingStrategy( _timer );
            return true;
        }

        IScrollingStrategy GetStrategyByName( string name )
        {
            switch ( name )
            {
                case "TurboScrollingStrategy":
                    if ( _strategies.ContainsKey( name ) ) return _strategies[name];
                    return new TurboScrollingStrategy( _timer, _registeredElements, Configuration );

                case "OneByOneScrollingStrategy":
                    if ( _strategies.ContainsKey( name ) ) return _strategies[name];
                    return new OneByOneScrollingStrategy( _timer, _registeredElements, Configuration );

                case "HalfZoneScrollingStrategy":
                    if ( _strategies.ContainsKey( name ) ) return _strategies[name];
                    return new HalfZoneScrollingStrategy( _timer, _registeredElements, Configuration );

                default:
                    if ( _strategies.ContainsKey( "BasicScrollingStrategy" ) ) return _strategies["BasicScrollingStrategy"];
                    return new ZoneScrollingStrategy( _timer, _registeredElements, Configuration );
            }
        }

        public void Start()
        {
            Configuration.ConfigChanged += OnConfigChanged;

            _currentTrigger = Configuration.User.GetOrSet( "Trigger", InputTrigger.Service.DefaultTrigger );
            InputTrigger.Service.RegisterFor( _currentTrigger, OnInputTriggered );
            _scrollingStrategy.Start();
        }

        private void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if ( e.MultiPluginId.Any( u => u.UniqueId == ScrollerPlugin.PluginId.UniqueId ) )
            {
                if ( e.Key == "Strategy" )
                {
                    _scrollingStrategy.Stop();
                   // _scrollingStrategy = GetStrategyByName( e.Value.ToString() );
                    _scrollingStrategy.Start();
                }
                if ( e.Key == "Trigger" )
                {
                    if ( _currentTrigger != null )
                    {
                        InputTrigger.Service.Unregister( _currentTrigger, OnInputTriggered );
                        _currentTrigger = Configuration.User.GetOrSet( "Trigger", InputTrigger.Service.DefaultTrigger );
                        InputTrigger.Service.RegisterFor( _currentTrigger, OnInputTriggered );
                    }
                }
            }
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

        /// <summary>
        /// This function forces the highlight of the given element.
        /// </summary>
        /// <param name="element">The element highlight.</param>
        /// <remarks> 
        /// This function is useful only when there is no alternative to.
        /// </remarks>
        public void HighlightImmediately( IHighlightableElement element )
        {
            _scrollingStrategy.GoToElement( element );
        }

        public bool IsHighlighting { get { return _timer.Enabled; } }

        public void RegisterTree( string targetModuleName, IHighlightableElement element, bool HighlightDirectly = false )
        {
            //if ( !_registeredElements.ContainsKey( targetModuleName ) )
            //{
            //    _registeredElements.Add( targetModuleName, element );
            //    if ( !_scrollingStrategy.IsStarted ) _scrollingStrategy.Start();
            //    if( HighlightDirectly ) _scrollingStrategy.GoToElement( element );
            //}
            _scrollingStrategy.AddModule( element );
        }

        public void UnregisterTree( string targetModuleName, IHighlightableElement element )
        {
            IHighlightableElement foundElement;
            if ( _registeredElements.TryGetValue( targetModuleName, out foundElement ) )
            {
                //If the element we're scrolling on is a proxy, we retrieve the actual element behind it.
                var ehep = foundElement as ExtensibleHighlightableElementProxy;
                if ( ehep != null && ehep != element ) foundElement = ehep.HighlightableElement;

                if ( foundElement == element )
                {
                    //Actually removing the module from the registered elements.
                    _registeredElements.Remove( targetModuleName );

                    BrowseTree( element, e =>
                    {
                        var iheus = e as IHighlightableElementController;
                        if ( iheus != null ) iheus.OnUnregisterTree();
                        return false;
                    } );

                    if ( _registeredElements.Count == 0 )
                    {
                        _scrollingStrategy.Stop();
                    }

                    //Warning the strategy that an element has been unregistered
                    _scrollingStrategy.ElementUnregistered( element );
                }
            }
        }

        /// <summary>
        /// Enables registering a tree node (and its children) in an already registered module.
        /// The module has to have an <see cref="IExtensibleHighlightableElement"/> whose name matches the extensibleElementName set as parameter.
        /// </summary>
        /// <param name="targetModuleName">The name of the target module</param>
        /// <param name="extensibleElementName">The name of the target node</param>
        /// <param name="position"></param>
        /// <param name="element">The element to register</param>
        /// <returns></returns>
        public bool RegisterInRegisteredElementAt( string targetModuleName, string extensibleElementName, ChildPosition position, IHighlightableElement element )
        {
            IHighlightableElement registeredElement;
            if ( _registeredElements.TryGetValue( targetModuleName, out registeredElement ) )
            {
                return BrowseTree( registeredElement, e =>
                {
                    IExtensibleHighlightableElement extensibleElement = e as IExtensibleHighlightableElement;
                    if ( extensibleElement != null && extensibleElement.Name == extensibleElementName )
                        return extensibleElement.RegisterElementAt( position, element );
                    else return false;
                } );
            }
            return false;
        }

        /// <summary>
        /// Enables unregistering a tree node (and its children) that had been registered inside an already registered module (see <see cref="RegisterInRegisteredElementAt"/> )
        /// </summary>
        /// <param name="targetModuleName">The name of the target module</param>
        /// <param name="extensibleElementName">The name of the target node</param>
        /// <param name="position"></param>
        /// <param name="element">The element to unregister</param>
        /// <returns></returns>
        public bool UnregisterInRegisteredElement( string targetModuleName, string extensibleElementName, ChildPosition position, IHighlightableElement element )
        {
            IHighlightableElement registeredElement;
            if ( _registeredElements.TryGetValue( targetModuleName, out registeredElement ) )
            {
                return BrowseTree( registeredElement, e =>
                {
                    IExtensibleHighlightableElement extensibleElement = e as IExtensibleHighlightableElement;
                    if ( extensibleElement != null && extensibleElement.Name == extensibleElementName ) return extensibleElement.UnregisterElement( position, element );
                    else return false;
                } );
            }
            return false;
        }

        /// <summary>
        /// This method calls the function with with "element" as parameter and then calls it on each of its hightlightable children.
        /// At any point, if the function returns false, the browsing stops.
        /// </summary>
        /// <param name="element">The element set as parameter of the function</param>
        /// <param name="doing">The function called on each node</param>
        /// <returns>true fi the browing has been stopped at any point</returns>
        bool BrowseTree( IHighlightableElement element, Func<IHighlightableElement, bool> doing )
        {
            if ( doing( element ) ) return true;

            foreach ( var child in element.Children )
            {
                if ( child.Children != null && child.Children.Count > 0 )
                {
                    if ( BrowseTree( child, doing ) ) return true;
                }
                if ( doing( child ) ) return true;
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

        #endregion

        private void OnInputTriggered( ITrigger t )
        {
            _scrollingStrategy.OnExternalEvent();
        }
    }
}
