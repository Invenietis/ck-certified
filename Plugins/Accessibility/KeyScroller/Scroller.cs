#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\KeyScroller\Scroller.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITriggerService> InputTrigger { get; set; }

        public event EventHandler<HighlightElementRegisterEventArgs> ElementRegisteredOrUnregistered;
        public event EventHandler<EventArgs> TriggerChanged;
        public event EventHandler<EventArgs> HighliterStatusChanged;
        public event EventHandler<HighlightEventArgs> BeginHighlight
        {
            add 
            { 
                _scrollingStrategy.BeginHighlightElement += value; 
            }
            remove { _scrollingStrategy.BeginHighlightElement -= value; }
        }

        public event EventHandler<HighlightEventArgs> EndHighlight
        {
            add { _scrollingStrategy.EndHighlightElement += value; }
            remove { _scrollingStrategy.EndHighlightElement -= value; }
        }

        public ITrigger Trigger
        {
            get { return _currentTrigger; }
            set
            {
                if( _currentTrigger == value ) return;
                _currentTrigger = value;
                FireTriggerChanged();
            }
        }

        public HighlighterStatus Status
        {
            get { return IsHighlighting ? HighlighterStatus.Highlighting : HighlighterStatus.Paused; }
        }

        IDictionary<string, string> IHighlighterService.RegisteredElements
        {
            get
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach( var item in _registeredElements )
                {
                    dic.Add( item.Key, _displayNames[item.Key] );
                }

                return dic;
            }
        }

        /// <summary>
        /// Gets all the elements that are registered into the scroller. also shows the ones that have been disabled through the configuration.
        /// </summary>
        public ICKReadOnlyList<IHighlightableElement> RegisteredElements
        {
            get
            {
                return _registeredElements.Values.ToReadOnlyList();
            }
        }

        public IReadOnlyCollection<IHighlightableElement> Elements
        {
            get { return _registeredElements.Values.ToArray(); }
        }

        /// <summary>
        /// Gets the <see cref="RegisteredElements"/> , minus the <see cref="DisabledElements"/> 
        /// </summary>
        public ICKReadOnlyList<IHighlightableElement> ScrollableElements
        {
            get
            {
                return _registeredElements.Where( kvp => !DisabledElements.Contains( kvp.Key ) )
                                          .Select( kvp => kvp.Value )
                                          .ToReadOnlyList();
            }
        }

        IEnumerable<string> _disabledElements;
        /// <summary>
        /// Gets the Elements that are registered into the scroller but have been disabled through the configuration.
        /// </summary>
        public IEnumerable<string> DisabledElements
        {
            get { return _disabledElements; }
        }

        public bool Setup( IPluginSetupInfo info )
        {
            _timer = new DispatcherTimer();

            _timer.Interval = new TimeSpan( 0, 0, 0, 0, Configuration.User.GetOrSet( "Speed", 1000 ) );
            
            _registeredElements = new Dictionary<string, IHighlightableElement>();
            var conf = Configuration.User.GetOrSet<ScrollingElementConfiguration>( "ScrollableModules", new ScrollingElementConfiguration() );
            _disabledElements = conf.Select( m => m.InternalName ).ToList();

            _scrollingStrategy = new StrategyBridge( _timer, () => ScrollableElements, Configuration );

            return true;
        }

        public void Start()
        {
            _scrollingStrategy.SwitchTo( Configuration.User.GetOrSet( "Strategy", "ZoneScrollingStrategy" ) );
            Configuration.ConfigChanged += OnConfigChanged;

            Trigger = Configuration.User.GetOrSet( "Trigger", InputTrigger.Service.DefaultTrigger );
            InputTrigger.Service.RegisterFor( Trigger, OnInputTriggered );
            _scrollingStrategy.StatusChanged += ( o, e ) =>
            {
                FireHighliterStatusChanged();
            };
        }

        private void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( u => u.UniqueId == ScrollerPlugin.PluginId.UniqueId ) )
            {
                if( e.Key == "Strategy" )
                {
                    _scrollingStrategy.SwitchTo( e.Value.ToString() );
                }
                else if( e.Key == "Trigger" )
                {
                    if( Trigger != null )
                    {
                        InputTrigger.Service.Unregister( Trigger, OnInputTriggered );
                        Trigger = Configuration.User.GetOrSet( "Trigger", InputTrigger.Service.DefaultTrigger );
                        InputTrigger.Service.RegisterFor( Trigger, OnInputTriggered );
                    }
                }
                else if( e.Key == "ScrollableModules" )
                {
                    //Console.Out.WriteLine( "Scrollable changed" );
                    ScrollingElementConfiguration conf = (ScrollingElementConfiguration)e.Value;
                    var disabledElements = conf.Select( m => m.InternalName ).ToList();

                    //Getting the removed items and propagating the fact that they have been removed
                    var removed = disabledElements.Except( _disabledElements );

                    foreach( var removedItem in removed )
                    {
                        if( _registeredElements.ContainsKey( removedItem ) )
                            _scrollingStrategy.ElementUnregistered( _registeredElements[removedItem] );
                    }

                    _disabledElements = disabledElements;
                }
            }
        }

        public void Stop()
        {
            InputTrigger.Service.Unregister( Trigger, OnInputTriggered );
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

        Dictionary<string, string> _displayNames = new Dictionary<string, string>();

        public void RegisterTree( string targetModuleName, string targetDisplayName, IHighlightableElement element, bool HighlightDirectly = false )
        {
            if( !_registeredElements.ContainsKey( targetModuleName ) )
            {
                if( _displayNames.ContainsKey( targetModuleName ) )
                    _displayNames[targetModuleName] = targetDisplayName;
                else
                    _displayNames.Add( targetModuleName, targetDisplayName );

                _registeredElements.Add( targetModuleName, element );
                if( !_scrollingStrategy.IsStarted ) _scrollingStrategy.Start();
                if( HighlightDirectly ) _scrollingStrategy.GoToElement( element );

                if( ElementRegisteredOrUnregistered != null ) ElementRegisteredOrUnregistered( this, new HighlightElementRegisterEventArgs( element, targetModuleName, true, element.IsHighlightableTreeRoot ) );
            }
        }

        public void UnregisterTree( string targetModuleName, IHighlightableElement element )
        {
            IHighlightableElement foundElement;
            if( _registeredElements.TryGetValue( targetModuleName, out foundElement ) )
            {
                //If the element we're scrolling on is a proxy, we retrieve the actual element behind it.
                var ehep = foundElement as ExtensibleHighlightableElementProxy;
                if( ehep != null && ehep != element ) foundElement = ehep.HighlightableElement;

                if( foundElement == element )
                {
                    //Actually removing the module from the registered elements.
                    _registeredElements.Remove( targetModuleName );

                    BrowseTree( element, e =>
                    {
                        var iheus = e as IHighlightableElementController;
                        if( iheus != null ) iheus.OnUnregisterTree();
                        return false;
                    } );

                    //Warning the strategy that an element has been unregistered
                    _scrollingStrategy.ElementUnregistered( ehep == null ? element : ehep );
                }
                if( ElementRegisteredOrUnregistered != null ) ElementRegisteredOrUnregistered( this, new HighlightElementRegisterEventArgs( element, targetModuleName, false, element.IsHighlightableTreeRoot ) );
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
            if( _registeredElements.TryGetValue( targetModuleName, out registeredElement ) )
            {
                return BrowseTree( registeredElement, e =>
                {
                    IExtensibleHighlightableElement extensibleElement = e as IExtensibleHighlightableElement;
                    if( extensibleElement != null && extensibleElement.ElementName == extensibleElementName )
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
            if( _registeredElements.TryGetValue( targetModuleName, out registeredElement ) )
            {
                _scrollingStrategy.ElementUnregistered( element );

                return BrowseTree( registeredElement, e =>
                {
                    IExtensibleHighlightableElement extensibleElement = e as IExtensibleHighlightableElement;
                    if( extensibleElement != null && extensibleElement.ElementName == extensibleElementName ) return extensibleElement.UnregisterElement( position, element );
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

        #endregion

        private void OnInputTriggered( ITrigger t )
        {
            FireOnTrigger();
            _scrollingStrategy.OnExternalEvent();
        }

        void FireOnTrigger()
        {
            if( OnTrigger != null )
                OnTrigger( this, new HighlightEventArgs( _scrollingStrategy.CurrentElement ) );
        }

        void FireTriggerChanged()
        {
            if(TriggerChanged != null) 
                TriggerChanged(this, new EventArgs());
        }

        void FireHighliterStatusChanged()
        {
            if( HighliterStatusChanged != null )
                HighliterStatusChanged( this, new EventArgs() );
        }

        #region IHighlighterService Members


        public event EventHandler<HighlightEventArgs> OnTrigger;

        #endregion
    }


    //class NamedHighlightableElement : IHighlightableElement
    //{
    //    IHighlightableElement _element;
    //    public string DisplayName { get; private set; }
    //    public NamedHighlightableElement( string displayName, IHighlightableElement element )
    //    {
    //        DisplayName = displayName;
    //        _element = element;
    //    }

    //    public override int GetHashCode()
    //    {
    //        return _element.GetHashCode();
    //    }

    //    public override string ToString()
    //    {
    //        return _element.ToString();
    //    }

    //    public override bool Equals( object obj )
    //    {
    //        return _element.Equals( obj );
    //    }

    //    #region IHighlightableElement Members

    //    public ICKReadOnlyList<IHighlightableElement> Children
    //    {
    //        get { return _element.Children; }
    //    }

    //    public int X
    //    {
    //        get { return _element.X; }
    //    }

    //    public int Y
    //    {
    //        get { return _element.Y; }
    //    }

    //    public int Width
    //    {
    //        get { return _element.Width; }
    //    }

    //    public int Height
    //    {
    //        get { return _element.Height; }
    //    }

    //    public SkippingBehavior Skip
    //    {
    //        get { return _element.Skip; }
    //    }

    //    public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
    //    {
    //        return _element.BeginHighlight( beginScrollingInfo, scrollingDirective );
    //    }

    //    public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
    //    {
    //        return _element.EndHighlight( endScrollingInfo, scrollingDirective );
    //    }

    //    public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
    //    {
    //        return _element.SelectElement( scrollingDirective );
    //    }

    //    public bool IsHighlightableTreeRoot
    //    {
    //        get { return _element.IsHighlightableTreeRoot; }
    //    }

    //    #endregion
    //}
}
