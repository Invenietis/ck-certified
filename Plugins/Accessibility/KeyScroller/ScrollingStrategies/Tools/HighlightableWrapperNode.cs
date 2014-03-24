using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HighlightModel;

namespace KeyScroller.ScrollingStrategies.Tools
{
    public class HighlightableWrapperNode : IHighlightableWrapperNode
    {
        IHighlightableWrapperNode _child;
        IHighlightableWrapperNode _next;
        IHighlightableWrapperNode _first;
        IHighlightableWrapperNode _last;

        public HighlightableWrapperNode(IHighlightableElement element )
        {
            Root = this;
            Highlightable = element;
        }

        public HighlightableWrapperNode( IHighlightableElement element, IHighlightableWrapperNode parent , IHighlightableWrapperNode root)
        {
            Highlightable = element;
            Parent = parent;
        }

        #region IHighlightableWrapperNode Members

        public IHighlightableWrapperNode Root { get; internal set; }

        public IHighlightableWrapperNode Parent { get; internal set; }

        public IHighlightableWrapperNode Child 
        { 
            get
            {
                if( _child == null && Highlightable.Children.Count > 0 )
                {
                    _child = new HighlightableWrapperNode( Highlightable.Children[0], this , Root);
                }

                return _child;
            }
        }

        public IHighlightableWrapperNode Next 
        { 
            get
            {
                int idx;
                if( _next == null && (idx = Parent.Highlightable.Children.IndexOf( Highlightable )) > -1 )
                {
                    IHighlightableElement elem = Parent.Highlightable.Children.ElementAt( idx + 1 );
                    if(elem != null )
                        _next = new HighlightableWrapperNode( elem, Parent, Root );
                }

                return _next;
            }
        }

        public IHighlightableWrapperNode First
        {
            get
            {
                if( _first == null && Parent != null )
                {
                    _first = new HighlightableWrapperNode( Parent.Highlightable.Children[0], Parent, Root );
                }

                return _first;
            }
        }
     

        public IHighlightableWrapperNode Last 
        {
            get
            {
                if( _last == null && Parent != null )
                {
                    _last = new HighlightableWrapperNode( Parent.Highlightable.Children[Parent.Highlightable.Children.Count - 1], Parent, Root );
                }

                return _last;
            } 
        }
    

        public HighlightModel.IHighlightableElement Highlightable { get; private set; }
  

        public bool HasChild
        {
            get { return Child != null; }
        }

        public bool IsRoot
        {
            get { return Root == this; }
        }

        #endregion
    }
}
