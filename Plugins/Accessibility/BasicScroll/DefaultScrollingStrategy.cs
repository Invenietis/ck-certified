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
    internal class DefaultScrollingStrategy
    {
        DispatcherTimer _timer;
        List<IHighlightableElement> _elements;
        IReadOnlyList<IHighlightableElement> _roElements;

        int _currentId = -1;
        Stack<IHighlightableElement> _currentElementParents = null;
        IHighlightableElement _currentElement = null;

        bool _enterChildren = false;

        internal event EventHandler<HighlightEventArgs> BeginHighlight;

        internal event EventHandler<HighlightEventArgs> EndHighlight;

        internal event EventHandler<HighlightEventArgs> SelectElement;

        public DefaultScrollingStrategy( DispatcherTimer timer, List<IHighlightableElement> elements )
        {
            _elements = elements;
            _timer = timer;

            _currentElementParents = new Stack<IHighlightableElement>();
            _timer.Tick += OnInternalBeat;
        }

        internal IReadOnlyList<IHighlightableElement> RegisteredElements
        {
            get { return _roElements ?? (_roElements = new ReadOnlyListOnIList<IHighlightableElement>( _elements )); }
        }

        void OnInternalBeat( object sender, EventArgs e )
        {
            if( _currentElement != null ) FireEndHighlight();
            
            // highlight the next element
            _currentElement = GetNextElement(_enterChildren);
            FireBeginHighlight();
            _enterChildren = false;
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
                if( _currentElement != null ) FireEndHighlight();
                _timer.Stop();
            }
        }

        internal void OnExternalEvent()
        {
            if( _currentElement.Children.Count > 0 ) _enterChildren = true;
            else
            {
                SelectElement( this, new HighlightEventArgs( _currentElement ) );
            }
        }

        IHighlightableElement GetNextElement(bool enterchild = false, bool moveToParent = false )
        {
            IHighlightableElement nextElement = null;

            if( moveToParent )
            {
                // if there is no parent, go to normal next element
                if( _currentElementParents.Count == 0 ) return GetNextElement();

                IHighlightableElement parent = _currentElementParents.Pop();
                IReadOnlyList<IHighlightableElement> parentSibblings = null;
                if( _currentElementParents.Count > 0 ) parentSibblings = _currentElementParents.Peek().Children;
                else parentSibblings = RegisteredElements;

                _currentId = parentSibblings.IndexOf( parent );
                nextElement = parent; 
            }
            else
            {
                IReadOnlyList<IHighlightableElement> elements = null;
                // get the sibblings of the current elements
                if( _currentElementParents.Count > 0 ) elements = _currentElementParents.Peek().Children;
                else elements = RegisteredElements;

                // if it's the first iteration, or if we just have to go to the next sibbling
                if( _currentId < 0 || !enterchild )
                {
                    if( _currentId < elements.Count - 1 ) _currentId++;
                    // if we are at the end of this elements set and if there is a parent in the stack, move to parent
                    else if( _currentElementParents.Count > 0 ) return GetNextElement( false, true );
                    // otherwise we go back to the first element
                    else _currentId = 0;

                    nextElement = elements[_currentId];
                }
                else
                {
                    // if the current element does not have childs ... go to the normal next element
                    if( elements[_currentId].Children.Count == 0 ) return GetNextElement();
                    // else we just push the element as a parent and set the the first child as the current element
                    _currentElementParents.Push( elements[_currentId] );
                    nextElement = elements[_currentId].Children[0];
                    _currentId = 0;
                }
            }

            switch( nextElement.Skip )
            {
                case SkippingBehavior.Skip:
                    return GetNextElement();
                case SkippingBehavior.EnterChildren:
                    return GetNextElement( true );
                default:
                    return nextElement;
            }
        }

        void FireBeginHighlight()
        {
            BeginHighlight( this, new HighlightEventArgs( _currentElement ) );
        }

        void FireEndHighlight()
        {
            EndHighlight( this, new HighlightEventArgs( _currentElement ) );
            _currentElement = null;
        }
    }
}
