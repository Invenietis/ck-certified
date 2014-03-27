using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using CK.Core;
using HighlightModel;

namespace KeyScroller
{
    public class ScrollingStrategy : IScrollingStrategy, IHighlightableElement
    {
        protected Timer _timer;
        protected ScrollingDirective _lastDirective;
        protected IHighlightableElement _previousElement = null;
        protected ITreeWalker _johnnie;
        protected Dictionary<string, IHighlightableElement> Elements { get; set; }
        bool _isStarted;
        public ScrollingStrategy( Timer timer, Dictionary<string, IHighlightableElement> elements )
        {
            _timer = timer;
            _johnnie = new TreeWalker();
            Elements = elements;
        }

        protected virtual void OnInternalBeat( object sender, EventArgs e )
        {
            if( _lastDirective == null ) _lastDirective = new ScrollingDirective( ActionType.Normal, ActionTime.NextTick );

            //Saving the currently highlighted element
            _previousElement = _johnnie.Current;

            //Fetching the next element
            MoveNext();
            Console.WriteLine( "BEAT" );
            //End highlight on the previous element (if different from the current one)
            if( _previousElement != null )
                FireEndHighlight( _previousElement, _johnnie.Current );

            //Begin highlight on the current element (even if the previous element is also the current element, we send the beginhighlight to give the component the beat)
            if( _johnnie.Current != null )
                FireBeginHighlight();
        }

        /// <summary>
        /// Calls the BeginHighlight method of the current IHighlightableElement
        /// It also sets _lastDirective to the ScrollingDirective object returned by the call to BeginHighlight.
        /// </summary>
        protected void FireBeginHighlight()
        {
            if( _johnnie.Current != null )
            {
                _lastDirective = _johnnie.Current.BeginHighlight( new BeginScrollingInfo( _timer.Interval, _previousElement ), _lastDirective );
                //EnsureReactivity();
            }
        }

        /// <summary>
        /// Calls the EndHighlight method of the current IHighlightableElement
        /// It also sets _lastDirective to the ScrollingDirective object returned by the call to EndHighlight.
        /// </summary>
        protected void FireEndHighlight( IHighlightableElement previousElement, IHighlightableElement element )
        {
            if( previousElement != null )
            {
                _lastDirective = previousElement.EndHighlight( new EndScrollingInfo( _timer.Interval, previousElement, element ), _lastDirective );
                //EnsureReactivity();
            }
        }

        public void MoveNext()
        {
            MoveNext( ActionType.Normal );
        }

        public void MoveNext(ActionType action)
        {
            if( _johnnie.Current == null) 
                _johnnie.GoTo( this );

            switch(action)
            {
                case ActionType.Normal :
                    if( !_johnnie.MoveNext() )
                        MoveNext(ActionType.UpToParent);
                    break;
                case ActionType.UpToParent:
                    if( !_johnnie.UpToParent() )
                        MoveNext(ActionType.GoToFirstSibling);
                    break;
                case ActionType.EnterChild :
                    if( !_johnnie.EnterChild() )
                        MoveNext();
                    break;
                case ActionType.GoToFirstSibling :
                    _johnnie.MoveFirst();
                    break;
                case ActionType.GoToLastSibling :
                    _johnnie.MoveLast();
                    break;
                default :
                     //Stay on the same
                    break;
            }

            ProcessSkipBehavior();
        }

        void ProcessSkipBehavior()
        {
            switch(_johnnie.Current.Skip)
            {
                case SkippingBehavior.Skip :
                    MoveNext();
                    break;
                case SkippingBehavior.EnterChildren :
                    MoveNext( ActionType.EnterChild );
                    break;
                default : 
                    break;
            }
        }

        #region IScrollingStrategy Members

        public bool IsStarted
        {
            get { return _isStarted; }
        }

        public string Name
        {
            get { return ""; }
        }

        public void Start()
        {
            if( IsStarted ) return;

            _timer.Elapsed += OnInternalBeat;
            _timer.Start();
            _isStarted = true;
        }

        public void Stop()
        {
            _timer.Elapsed -= OnInternalBeat;
            _timer.Stop();
            _isStarted = false;
        }

        public void GoToElement( HighlightModel.IHighlightableElement element )
        {
            _johnnie.GoTo( element );
        }

        public void Pause( bool forceEndHighlight )
        {
        }

        public void Resume()
        {
        }

        public void OnExternalEvent()
        {

        }

        public void ElementUnregistered( HighlightModel.IHighlightableElement element )
        {
        }

        #endregion

        #region IHighlightableElement Members

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return new CKReadOnlyListOnIList<IHighlightableElement>(Elements.Values.ToList()); }
        }

        public int X
        {
            get { return 0; }
        }

        public int Y
        {
            get { return 0; }
        }

        public int Width
        {
            get { return 0; }
        }

        public int Height
        {
            get { return 0; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.EnterChildren; }
        }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return false; }
        }

        #endregion
    }
}
