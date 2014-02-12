using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin.Config;
using HighlightModel;
using System.Diagnostics;

namespace KeyScroller
{
    public abstract class ScrollingStrategyBase : IScrollingStrategy
    {
        ICKReadOnlyList<IHighlightableElement> _roElements;

        protected Dictionary<string, IHighlightableElement> _elements;
        protected IPluginConfigAccessor _configuration;
        protected DispatcherTimer _timer;
        protected int _currentId = -1;
        protected int _repeatCount = 1;

        protected Stack<IHighlightableElement> _currentElementParents = null;
        protected IHighlightableElement _previousElement = null;
        protected IHighlightableElement _currentElement = null;
        protected ScrollingDirective _lastDirective;

        internal ICKReadOnlyList<IHighlightableElement> RegisteredElements
        {
            // ToDOJL
            //get { return _roElements ?? ( _roElements = _elements.Values.ToReadOnlyList() ); }
            get { return _roElements = _elements.Values.ToReadOnlyList(); }
        }

        public ScrollingStrategyBase(DispatcherTimer timer, Dictionary<string, IHighlightableElement> elements, IPluginConfigAccessor configuration)
        {
            _elements = elements;
            _timer = timer;
            _currentElementParents = new Stack<IHighlightableElement>();
            _configuration = configuration;
        }

        /// <summary>
        /// Goes up in the tree and returns the root of the first registered tree
        /// </summary>
        /// <returns>The root of the first registered tree</returns>
        protected virtual IHighlightableElement GetUpToAbsoluteRoot()
        {
            _currentId = -1;
            _currentElementParents = new Stack<IHighlightableElement>();

            return RegisteredElements.FirstOrDefault();
        }

        /// <summary>
        /// Goes up in the tree and returns the first child of the relative root of the current element's tree 
        /// For example : the RelativeRoot of the keyboard is the VMKeyboard itself. We are going to get the keyboard's first child and to start iterating on it directly. 
        /// </summary>
        /// <returns>The first child of the current element's relative root</returns>
        protected virtual IHighlightableElement GetUpToRelativeRoot()
        {
            if( _currentElementParents.Count == 0 ) return _currentElement;

            //Getting the children of the root element of the current tree

            ICKReadOnlyList<IHighlightableElement> rootChildren = null;
            while( _currentElementParents.Count > 1 )
            {
                _currentElementParents.Pop();
            }
            rootChildren = _currentElementParents.Peek().Children;

            //Returning the first child.
            _currentId = -1;
            return rootChildren.First();
        }

        protected virtual IHighlightableElement GetUpToParent()
        {
            IHighlightableElement nextElement = null;
            // if there is no parent, go to normal next element
            if( _currentElementParents.Count == 0 ) return GetNextElement( ActionType.Normal );

            //We get the parent and fetch its siblings
            IHighlightableElement parent = _currentElementParents.Pop();
            ICKReadOnlyList<IHighlightableElement> parentSiblings = null;
            if( _currentElementParents.Count > 0 )
            {
                parentSiblings = _currentElementParents.Peek().Children;
            }
            else
            {
                //there, we actually are at the root level
                Debug.Assert( parent.IsHighlightableTreeRoot );
                parentSiblings = RegisteredElements;

                //If this tree is the only tree at the root level, we directly start iterating on its children
                if( parentSiblings.Count == 1 )
                {
                    _currentId = 0;
                    return GetNextElement( ActionType.EnterChild );
                }
            }

            _currentId = parentSiblings.IndexOf( parent );
            nextElement = parent;

            // if the parent skipping behavior is enter children, we skip it
            if( parent.Skip == SkippingBehavior.EnterChildren )
            {
                _currentElement = nextElement;
                return GetNextElement( ActionType.Normal );
            }

            return nextElement;
        }

        protected virtual IHighlightableElement GetStayOnTheSame( ICKReadOnlyList<IHighlightableElement> elements )
        {
            return elements[_currentId];
        }

        protected virtual IHighlightableElement GetSkipBehavior( IHighlightableElement element )
        {
            switch( element.Skip )
            {
                case SkippingBehavior.Skip:
                    return GetNextElement( ActionType.Normal );
                case SkippingBehavior.EnterChildren:
                    return GetNextElement( ActionType.EnterChild );
                default:
                    return element;
            }
        }

        protected virtual IHighlightableElement GetEnterChild( ICKReadOnlyList<IHighlightableElement> elements )
        {
            // if the current element does not have any children ... go to the normal next element
            if( elements[_currentId].Children.Count == 0 ) return GetNextElement( ActionType.Normal );
            // otherwise we just push the element as a parent and set the first child as the current element
            _currentElementParents.Push( elements[_currentId] );
            int tmpId = _currentId;
            _currentId = 0;

            return elements[tmpId].Children[0];
        }

        protected virtual IHighlightableElement GetGoToFirstSibling( ICKReadOnlyList<IHighlightableElement> elements )
        {
            _currentId = 0;
            return elements[_currentId];
        }

        protected virtual IHighlightableElement GetGoToLastSibling( ICKReadOnlyList<IHighlightableElement> elements )
        {
            _currentId = elements.Count - 1;
            return elements[_currentId];
        }

        protected virtual IHighlightableElement GetNextElement( ActionType actionType )
        {
            // reset the action type to normal if we are not on a StayOnTheSameLocked
            if( actionType != ActionType.StayOnTheSameLocked )
                _lastDirective.NextActionType = ActionType.Normal;

            //We retrieve parents that implement IHighlightableElementController
            var contrillingParents = _currentElementParents.Where((i) => { return i is IHighlightableElementController; });

            foreach (IHighlightableElementController parent in contrillingParents)
            {
                //we offer the possibility to change action
                actionType = parent.PreviewChildAction( _currentElement, actionType);
            }

            foreach( IHighlightableElementController parent in contrillingParents )
            {
                //inform that there is a new ActionType
                parent.OnChildAction( actionType );
            }

            IHighlightableElement nextElement = null;

            if( actionType == ActionType.AbsoluteRoot )
            {
                nextElement = GetUpToAbsoluteRoot();
            }
            else if( actionType == ActionType.RelativeRoot )
            {
                nextElement = GetUpToRelativeRoot();
            }
            else if( actionType == ActionType.UpToParent )
            {
                nextElement = GetUpToParent();
            }
            else
            {
                ICKReadOnlyList<IHighlightableElement> elements = null;
                // get the sibling of the current element
                if( _currentElementParents.Count > 0 ) elements = _currentElementParents.Peek().Children;
                else
                {
                    //We are on the root level
                    elements = RegisteredElements;

                    // ToDoJL
                    //if( actionType != ActionType.EnterChild && elements.Count == 1 )//We are on the root level, and there is only one element, so we directly enter it.
                    //{
                    //    _currentId = 0;
                    //    nextElement = GetEnterChild( elements );
                    //    return GetSkipBehavior( nextElement );
                    //}
                }

                if( actionType == ActionType.StayOnTheSameOnce || actionType == ActionType.StayOnTheSameLocked )
                {
                    nextElement = GetStayOnTheSame( elements );
                }
                else if( actionType == ActionType.GoToFirstSibling )
                {
                    nextElement = GetGoToFirstSibling( elements );
                }
                else if( actionType == ActionType.GoToLastSibling )
                {
                    nextElement = GetGoToLastSibling( elements );
                }
                else if( _currentId < 0 || actionType != ActionType.EnterChild )
                {
                    // if it's the first iteration, or if we just have to go to the next sibbling
                    if( _currentId < elements.Count - 1 ) _currentId++;
                    // if we are at the end of this elements set and if there is a parent in the stack, move to parent
                    else if( _currentElementParents.Count > 0 ) return GetNextElement( ActionType.UpToParent );
                    // otherwise we go back to the first element
                    else _currentId = 0;

                    nextElement = elements[_currentId];
                }
                else
                {
                    nextElement = GetEnterChild( elements );
                }
            }

            return GetSkipBehavior( nextElement );
        }

        bool _isStarted;
        public bool IsStarted { get { return _isStarted; } }
        public virtual void Start()
        {
            Debug.Assert( !_isStarted );

            StartTimer();

            _timer.Tick += OnInternalBeat;
            _configuration.ConfigChanged += OnConfigChanged;
            _isStarted = true;
        }

        public virtual void Stop()
        {
            if( _timer.IsEnabled )
            {
                FireEndHighlight( _currentElement, null );
                _timer.IsEnabled = false;
            }
            _timer.Tick -= OnInternalBeat;
            _configuration.ConfigChanged -= OnConfigChanged;
            _isStarted = false;
        }

        public virtual void Pause( bool forceEndHighlight )
        {
            if( _timer.IsEnabled )
            {
                if( forceEndHighlight )
                {
                    FireEndHighlight( _currentElement, null );
                }
                _timer.Stop();
            }
        }

        public virtual void Resume()
        {
            StartTimer();
        }

        protected virtual void StartTimer()
        {
            if( !_timer.IsEnabled && _elements.Count > 0 )
            {
                _timer.Start();
            }
        }

        protected virtual void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( u => u.UniqueId == KeyScrollerPlugin.PluginId.UniqueId ) )
            {
                if( e.Key == "Speed" )
                {
                    _timer.Interval = new TimeSpan( 0, 0, 0, 0, (int)e.Value );
                }
            }
        }

        public abstract void OnExternalEvent();

        protected virtual void OnInternalBeat( object sender, EventArgs e )
        {
            if( _lastDirective == null ) _lastDirective = new ScrollingDirective( ActionType.Normal, ActionTime.NextTick );

            //Console.Out.WriteLine( "BEAT ! Date : " + DateTime.UtcNow.Second );

            //Saving the currently highlighted element
            _previousElement = _currentElement;

            //Fetching the next element
            _currentElement = GetNextElement( _lastDirective.NextActionType );

            //End highlight on the previous element (if different from the current one)
            if( _previousElement != null )
                FireEndHighlight( _previousElement, _currentElement );

            //Begin highlight on the current element (even if the previous element is also the current element, we send the beginhighlight to give the component the beat)
            if( _currentElement != null )
                FireBeginHighlight();
        }

        /// <summary>
        /// Calls the SelectElement method of the current IHighlightableElement
        /// It also sets _lastDirective to the ScrollingDirective object returned by the call to SelectElement.
        /// </summary>
        protected void FireSelectElement()
        {
            if( _currentElement != null )
            {
                _lastDirective = _currentElement.SelectElement( _lastDirective );

                EnsureReactivity();
            }
        }

        /// <summary>
        /// if the directive is to react instantly, we stop the timer, simulate a tick, and relaunch the timer.
        /// </summary>
        internal void EnsureReactivity()
        {
            if( _lastDirective != null && _lastDirective.ActionTime == ActionTime.Immediate )
            {
                //Setting the ActionTime back to NextTick. Immediate has to be set explicitely at each step;
                _lastDirective.ActionTime = ActionTime.NextTick;

                _timer.Stop();
                OnInternalBeat( this, EventArgs.Empty );
                //Console.Out.WriteLine( "Immediate !" );
                _timer.Start();
            }
        }

        public abstract string Name
        {
            get;
        }

        public void ElementUnregistered( IHighlightableElement unregisteredElement )
        {
            if( _currentElementParents.Contains( unregisteredElement ) )
            {
                //The unregistered element is one of the parents of the current element, so we need to stop iterating on this element and start on the next one.
                FireEndHighlight( _currentElement, null );

                //We flush the parent list. When we call the next element, we'll be on the next registered tree
                _currentElementParents = new Stack<IHighlightableElement>();
                _currentId = 0;
            }
        }

        /// <summary>
        /// Calls the BeginHighlight method of the current IHighlightableElement
        /// It also sets _lastDirective to the ScrollingDirective object returned by the call to BeginHighlight.
        /// </summary>
        void FireBeginHighlight()
        {
            if( _currentElement != null )
            {
                _lastDirective = _currentElement.BeginHighlight( new BeginScrollingInfo( _timer.Interval, _previousElement ), _lastDirective );
                EnsureReactivity();
            }
        }

        /// <summary>
        /// Calls the EndHighlight method of the current IHighlightableElement
        /// It also sets _lastDirective to the ScrollingDirective object returned by the call to EndHighlight.
        /// </summary>
        void FireEndHighlight( IHighlightableElement previouslyHighlightedElement, IHighlightableElement elementToBeHighlighted )
        {
            if( previouslyHighlightedElement != null )
            {
                _lastDirective = previouslyHighlightedElement.EndHighlight( new EndScrollingInfo( _timer.Interval, previouslyHighlightedElement, elementToBeHighlighted ), _lastDirective );
                EnsureReactivity();
            }
        }
    }
}
