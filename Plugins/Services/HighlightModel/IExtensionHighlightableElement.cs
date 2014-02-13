using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightModel
{
    public interface IExtensibleHighlightableElement : IHighlightableElement
    {
        bool RegisterElementAt( ChildPosition position, IHighlightableElement child );

        bool UnregisterElement( ChildPosition position, IHighlightableElement element );

        string Name { get; }

        IReadOnlyList<IHighlightableElement> PreChildren { get; }

        IReadOnlyList<IHighlightableElement> PostChildren { get; }
    }
}