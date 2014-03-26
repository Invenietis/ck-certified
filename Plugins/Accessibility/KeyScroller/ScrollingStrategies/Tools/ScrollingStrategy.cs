using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using HighlightModel;

namespace KeyScroller
{
    public class ScrollingStrategy : IScrollingStrategy
    {
        protected Timer _timer;
        protected ScrollingDirective _lastDirective;
        protected IHighlightableWrapperNode _previousNode = null;

        public ScrollingStrategy( Timer timer )
        {
            Root = new HighlightableWrapperRoot();
            _timer = timer;
        }

        public IHighlightableWrapperNode CurrentNode { get; internal set; }
        public HighlightableWrapperRoot Root { get; internal set; }

        public void AddModule( IHighlightableElement element )
        {
            Root.AddChild( element );
        }

        protected virtual void OnInternalBeat( object sender, EventArgs e )
        {
            if( _lastDirective == null ) _lastDirective = new ScrollingDirective( ActionType.Normal, ActionTime.NextTick );

            //Saving the currently highlighted element
            _previousNode = CurrentNode;

            //Fetching the next element
            CurrentNode = CurrentNode == Root ? Next( ActionType.EnterChild) :  Next( _lastDirective.NextActionType );

            //End highlight on the previous element (if different from the current one)
            if( _previousNode != null )
                FireEndHighlight( _previousNode, CurrentNode );

            //Begin highlight on the current element (even if the previous element is also the current element, we send the beginhighlight to give the component the beat)
            if( CurrentNode != null )
                FireBeginHighlight();
        }

        /// <summary>
        /// Calls the BeginHighlight method of the current IHighlightableElement
        /// It also sets _lastDirective to the ScrollingDirective object returned by the call to BeginHighlight.
        /// </summary>
        protected void FireBeginHighlight()
        {
            if( CurrentNode != null )
            {
                _lastDirective = CurrentNode.Highlightable.BeginHighlight( new BeginScrollingInfo( _timer.Interval, _previousNode.Highlightable ), _lastDirective );
                //EnsureReactivity();
            }
        }

        /// <summary>
        /// Calls the EndHighlight method of the current IHighlightableElement
        /// It also sets _lastDirective to the ScrollingDirective object returned by the call to EndHighlight.
        /// </summary>
        protected void FireEndHighlight( IHighlightableWrapperNode previousNode, IHighlightableWrapperNode node )
        {
            if( previousNode != null )
            {
                _lastDirective = previousNode.Highlightable.EndHighlight( new EndScrollingInfo( _timer.Interval, previousNode.Highlightable, node.Highlightable ), _lastDirective );
                //EnsureReactivity();
            }
        }

        public IHighlightableWrapperNode Next()
        {
            return Next( ActionType.Normal );
        }

        public IHighlightableWrapperNode Next(ActionType action)
        {
            IHighlightableWrapperNode nexElement;

            switch(action)
            {
                case ActionType.Normal :
                    if( CurrentNode.Next != null )
                        nexElement = CurrentNode.Next;
                    else
                        nexElement = CurrentNode.Parent;
                    break;
                case ActionType.UpToParent:
                    if( CurrentNode.Parent != null )
                        nexElement = CurrentNode.Parent;
                    else 
                        nexElement = CurrentNode.First;
                    break;
                case ActionType.EnterChild :
                    if( CurrentNode.Child != null )
                        nexElement = CurrentNode.Child;
                    else
                        nexElement = CurrentNode.Next;
                    break;
                case ActionType.GoToFirstSibling :
                    nexElement = CurrentNode.First;
                    break;
                case ActionType.GoToLastSibling :
                    nexElement = CurrentNode.Last;
                    break;
                default :
                    nexElement = CurrentNode; //Stay on the same
                    break;
            }

            return GetBySkipBehavior(nexElement);
        }


        IHighlightableWrapperNode GetBySkipBehavior( IHighlightableWrapperNode node )
        {
            if( node.Highlightable.Skip == SkippingBehavior.None ) return node;
            if( node.Highlightable.Skip == SkippingBehavior.Skip ) return node.Next;
            return node.Child;
        }

        #region IScrollingStrategy Members

        public bool IsStarted
        {
            get { return _timer.Enabled; }
        }

        public string Name
        {
            get { return ""; }
        }

        public void Start()
        {
            
            CurrentNode = (IHighlightableWrapperNode) Root;
            _timer.Elapsed += OnInternalBeat;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Elapsed -= OnInternalBeat;
        }

        public void GoToElement( HighlightModel.IHighlightableElement element )
        {
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
    }
}
