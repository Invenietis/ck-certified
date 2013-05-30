using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using CK.Core;
using CommonServices.Accessibility;
using HighlightModel;

namespace BasicScroll
{
    enum ActionType
    {
        Normal = 0,
        EnterChild = 1,
        UpToParent = 2,
        StayOnTheSame = 3
    }

    internal class DefaultScrollingStrategy
    {
        DispatcherTimer _timer;
        List<IHighlightableElement> _elements;
        ICKReadOnlyList<IHighlightableElement> _roElements;

        int _currentId = -1;
        Stack<IHighlightableElement> _currentElementParents = null;
        IHighlightableElement _currentElement = null;

        ActionType _actionType = ActionType.Normal;

        internal event EventHandler<HighlightEventArgs> BeginHighlight;

        internal event EventHandler<HighlightEventArgs> EndHighlight;

        internal event EventHandler<HighlightEventArgs> SelectElement;

        /// <summary>
        /// Used when the 
        /// </summary>
        public void ElementUnregistered( IHighlightableElement unregisteredElement, bool getNext )
        {
            if( _currentElementParents.Contains( unregisteredElement ) )
            {
                //The unregistered element is one of the parents of the current element, so we need to stop iterating on this element and start on the next one.
                if( _currentElement != null ) FireEndHighlight();

                //We flush the parent list. When we call the next element, we'll be on the next registered tree
                _currentElementParents = new Stack<IHighlightableElement>();
                _currentId = 0;
                if( getNext ) GetNextElement( ActionType.Normal );
            }
        }

        public DefaultScrollingStrategy( DispatcherTimer timer, List<IHighlightableElement> elements )
        {
            _elements = elements;
            _timer = timer;

            _currentElementParents = new Stack<IHighlightableElement>();
            _timer.Tick += OnInternalBeat;
        }

        internal ICKReadOnlyList<IHighlightableElement> RegisteredElements
        {
            get { return _roElements ?? ( _roElements = new CKReadOnlyListOnIList<IHighlightableElement>( _elements ) ); }
        }

        void OnInternalBeat( object sender, EventArgs e )
        {
            if( _currentElement != null ) FireEndHighlight();

            // highlight the next element
            _currentElement = GetNextElement( _actionType );
            FireBeginHighlight();
        }

        internal void Start()
        {
            if( !_timer.IsEnabled && _elements.Count > 0 )
            {
                _timer.Start();
            }
        }

        internal void Stop()
        {
            if( _timer.IsEnabled )
            {
                if( _currentElement != null )
                {
                    FireEndHighlight();
                }
                _timer.IsEnabled = false;
            }
        }

        internal void OnExternalEvent()
        {
            if( _currentElement != null )
            {
                if( _currentElement.Children.Count > 0 ) _actionType = ActionType.EnterChild;
                else
                {
                    SelectElement( this, new HighlightEventArgs( _currentElement ) );
                    _actionType = ActionType.StayOnTheSame;
                }
            }
        }

        IHighlightableElement GetNextElement( ActionType actionType )
        {
            // reset the action type to normal
            _actionType = ActionType.Normal;

            IHighlightableElement nextElement = null;

            if( actionType == ActionType.UpToParent )
            {
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
            }
            else
            {
                ICKReadOnlyList<IHighlightableElement> elements = null;
                // get the sibblings of the current elements
                if( _currentElementParents.Count > 0 ) elements = _currentElementParents.Peek().Children;
                else elements = RegisteredElements;

                if( actionType == ActionType.StayOnTheSame )
                {
                    nextElement = elements[_currentId];
                    _actionType = ActionType.UpToParent;
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
                    // if the current element does not have childs ... go to the normal next element
                    if( elements[_currentId].Children.Count == 0 ) return GetNextElement( ActionType.Normal );
                    // else we just push the element as a parent and set the the first child as the current element
                    _currentElementParents.Push( elements[_currentId] );
                    nextElement = elements[_currentId].Children[0];
                    _currentId = 0;
                }
            }

            switch( nextElement.Skip )
            {
                case SkippingBehavior.Skip:
                    return GetNextElement( ActionType.Normal );
                case SkippingBehavior.EnterChildren:
                    return GetNextElement( ActionType.EnterChild );
                default:
                    return nextElement;
            }
        }

        void FireBeginHighlight()
        {
            if( BeginHighlight != null ) BeginHighlight( this, new HighlightEventArgs( _currentElement ) );
        }

        void FireEndHighlight()
        {
            if( EndHighlight != null ) EndHighlight( this, new HighlightEventArgs( _currentElement ) );
            _currentElement = null;
        }
    }
}
