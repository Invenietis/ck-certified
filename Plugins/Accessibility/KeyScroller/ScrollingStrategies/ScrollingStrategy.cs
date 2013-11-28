using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin.Config;
using CommonServices.Accessibility;
using HighlightModel;
using System.Diagnostics;

namespace KeyScroller
{
    public enum ActionType
    {
        Normal = 0,
        EnterChild = 1,
        UpToParent = 2,
        StayOnTheSame = 3
    }

    public abstract class ScrollingStrategy : IScrollingStrategy
    {
        ICKReadOnlyList<IHighlightableElement> _roElements;

        protected List<IHighlightableElement> _elements;
        protected DispatcherTimer _timer;
        protected IPluginConfigAccessor _configuration;

        protected int _currentId = -1;
        protected Stack<IHighlightableElement> _currentElementParents = null;
        protected IHighlightableElement _currentElement = null;
        protected ActionType _actionType = ActionType.EnterChild;

        internal ICKReadOnlyList<IHighlightableElement> RegisteredElements
        {
            get { return _roElements ?? ( _roElements = new CKReadOnlyListOnIList<IHighlightableElement>( _elements ) ); }
        }

        public ScrollingStrategy( DispatcherTimer timer, List<IHighlightableElement> elements, IPluginConfigAccessor configuration )
        {
            _elements = elements;
            _timer = timer;
            _currentElementParents = new Stack<IHighlightableElement>();
            _configuration = configuration;
        }

        #region IScrollingStrategy Members

        public event EventHandler<HighlightEventArgs> BeginHighlight;

        public event EventHandler<HighlightEventArgs> EndHighlight;

        public event EventHandler<HighlightEventArgs> SelectElement;

        protected void FireSelectElement( object sender, HighlightEventArgs eventArgs )
        {
            SelectElement( sender, eventArgs );
        }
        protected virtual void OnInternalBeat( object sender, EventArgs e )
        {
            //Console.Out.WriteLine( "Internalbeat " + DateTime.Now );
            if( _currentElement != null ) FireEndHighlight();

            // highlight the next element
            _currentElement = GetNextElement( _actionType );
            FireBeginHighlight();
        }

        protected virtual IHighlightableElement GetUpToParent()
        {
            IHighlightableElement nextElement = null;
            // if there is no parent, go to normal next element
            if( _currentElementParents.Count == 0 ) return GetNextElement( ActionType.Normal );

            IHighlightableElement parent = _currentElementParents.Pop();
            ICKReadOnlyList<IHighlightableElement> parentSibblings = null;
            if( _currentElementParents.Count > 0 ) parentSibblings = _currentElementParents.Peek().Children;
            else parentSibblings = RegisteredElements;

            _currentId = parentSibblings.IndexOf( parent );
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
            _actionType = ActionType.Normal;
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
            // otherwise we just push the element as a parent and set the the first child as the current element
            _currentElementParents.Push( elements[_currentId] );
            int tmpId = _currentId;
            _currentId = 0;

            return elements[tmpId].Children[0];
        }

        protected virtual IHighlightableElement GetNextElement( ActionType actionType )
        {
            // reset the action type to normal
            _actionType = ActionType.Normal;

            IHighlightableElement nextElement = null;

            if( actionType == ActionType.UpToParent )
            {
                nextElement = GetUpToParent();
            }
            else
            {
                ICKReadOnlyList<IHighlightableElement> elements = null;
                // get the sibling of the current element
                if( _currentElementParents.Count > 0 ) elements = _currentElementParents.Peek().Children;
                else elements = RegisteredElements;

                if( actionType == ActionType.StayOnTheSame )
                {
                    nextElement = GetStayOnTheSame( elements );
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

            //Console.Out.WriteLine( "Registering " + Name );
            _timer.Tick += OnInternalBeat;
            _configuration.ConfigChanged += OnConfigChanged;
            _isStarted = true;
        }

        public virtual void Stop()
        {
            if( _timer.IsEnabled )
            {
                if( _currentElement != null )
                {
                    FireEndHighlight();
                }
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
                if( forceEndHighlight && _currentElement != null )
                {
                    FireEndHighlight();
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

        #endregion

        void FireBeginHighlight()
        {
            if( BeginHighlight != null ) BeginHighlight( this, new HighlightEventArgs( _currentElement ) );
        }

        void FireEndHighlight()
        {
            if( EndHighlight != null ) EndHighlight( this, new HighlightEventArgs( _currentElement ) );
            _currentElement = null;
        }

        #region IScrollingStrategy Members

        public abstract string Name
        {
            get;
        }

        public void ElementUnregistered( IHighlightableElement unregisteredElement )
        {
            if( _currentElementParents.Contains( unregisteredElement ) )
            {
                //The unregistered element is one of the parents of the current element, so we need to stop iterating on this element and start on the next one.
                if( _currentElement != null ) FireEndHighlight();

                //We flush the parent list. When we call the next element, we'll be on the next registered tree
                _currentElementParents = new Stack<IHighlightableElement>();
                _currentId = 0;
            }
        }

        #endregion
    }
}
