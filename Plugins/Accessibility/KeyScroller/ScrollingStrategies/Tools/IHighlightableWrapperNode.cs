using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HighlightModel;

namespace KeyScroller
{
    public interface IHighlightableWrapperNode
    {
        IHighlightableWrapperNode Root { get; }
        IHighlightableWrapperNode Parent { get; }
        IHighlightableWrapperNode Child { get; }
        IHighlightableWrapperNode Next { get; }
        IHighlightableWrapperNode First { get; }
        IHighlightableWrapperNode Last { get; }
        IHighlightableElement Highlightable { get; }
        bool HasChild { get; }
        bool IsRoot { get; }
    }
}
