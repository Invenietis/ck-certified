using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScreenScroller
{
    public interface IRootNode
    {
        NodeViewModel CurrentNode { get; }
    }
}
