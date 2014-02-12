using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightModel
{
    public interface IHighlightableElementRoot
    {
        void RegisterTreeAt( int index, IHighlightableElement child );

        void UnregisterTree( IHighlightableElement element );
    }
}