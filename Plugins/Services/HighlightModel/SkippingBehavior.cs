using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightModel
{
    public enum SkippingBehavior
    {
        /// <summary>
        /// The element will not be skipped.
        /// </summary>
        None = 0,
        /// <summary>
        /// The element will be absolutely skipped, even its children.
        /// </summary>
        Skip = 1,
        /// <summary>
        /// The element will be skipped but the iterator will go to its children directly.
        /// </summary>
        EnterChildren = 2
    }
}
