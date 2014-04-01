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

namespace Scroller
{
    [Plugin( ScrollerPlugin.PluginIdString,
           PublicName = PluginPublicName,
           Version = ScrollerPlugin.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class ScrollerPlugin : IPlugin, IHighlighterService
    {
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        internal const string PluginIdString = "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Scroller";

        Dictionary<string, IHighlightableElement> _registeredElements;
        StrategyBridge _scrollingStrategy;
        DispatcherTimer _timer;
        ITrigger _currentTrigger;

        public IPluginConfigAccessor Configuration { get; set; }

        public IReadOnlyList<IHighlightableElement> RegisteredElements
        {
            get { return _registeredElements.Values.ToReadOnlyList(); }
        }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITriggerService> InputTrigger { get; set; }


        public bool Setup( IPluginSetupInfo info )
        {
            _timer = new DispatcherTimer();

            _timer.Interval = new TimeSpan(0, 0, 0, 0, Configuration.User.GetOrSet( "Speed", 1000 ));

            _registeredElements = new Dictionary<string, IHighlightableElement>();

            _scrollingStrategy = new StrategyBridge( _timer, _registeredElements, Configuration );
            
            return true;
        }

        public void Start()
        {
            _scrollingStrategy.SwitchTo( Configuration.User.GetOrSet( "Strategy", "ZoneScrollingStrategy" ) );
            Configuration.ConfigChanged += OnConfigChanged;

            _currentTrigger = Configuration.User.GetOrSet( "Trigger", InputTrigger.Service.DefaultTrigger );
            InputTrigger.Service.RegisterFor( _currentTrigger, OnInputTriggered );
            
        }

        private void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if ( e.MultiPluginId.Any( u => u.UniqueId == ScrollerPlugin.PluginId.UniqueId ) )
            {
                if ( e.Key == "Strategy" )
                {
                    _scrollingStrategy.SwitchTo( e.Value.ToString() );
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

        public bool IsHighlighting { get { return _timer.IsEnabled; } }

        public void RegisterTree( string targetModuleName, IHighlightableElement element, bool HighlightDirectly = false )
        {
            if( !_registeredElements.ContainsKey( targetModuleName ) )
            {
                _registeredElements.Add( targetModuleName, element );
                if( !_scrollingStrategy.IsStarted ) _scrollingStrategy.Start();
                if( HighlightDirectly ) _scrollingStrategy.GoToElement( element );
            }

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
                _scrollingStrategy.ElementUnregistered( element );

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
