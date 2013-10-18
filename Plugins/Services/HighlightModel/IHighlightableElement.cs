using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace HighlightModel
{
    public interface IHighlightableElement
    {
        /// <summary>
        /// Gets the collection of inner children of this element.
        /// </summary>
        ICKReadOnlyList<IHighlightableElement> Children { get; }

        /// <summary>
        /// Gets the X position of this element. This property can be used by scroll algorythms to determine the scroll scenario.
        /// <remarks>X, Y, Width and Height can be "virtual" values, not necessary pixels positions on the screen</remarks>
        /// </summary>
        int X { get; }

        /// <summary>
        /// Gets the Y position of this element. This property can be used by scroll algorythms to determine the scroll scenario.
        /// <remarks>X, Y, Width and Height can be "virtual" values, not necessary pixels positions on the screen</remarks>
        int Y { get; }

        /// <summary>
        /// Gets the width of this element. This property can be used by scroll algorythms to determine the scroll scenario.
        /// <remarks>X, Y, Width and Height can be "virtual" values, not necessary pixels positions on the screen</remarks>
        int Width { get; }

        /// <summary>
        /// Gets the height of this element. This property can be used by scroll algorythms to determine the scroll scenario.
        /// <remarks>X, Y, Width and Height can be "virtual" values, not necessary pixels positions on the screen</remarks>
        int Height { get; }

        /// <summary>
        /// Gets if the element has to be skipped by the scoll algorythm.
        /// </summary>
        SkippingBehavior Skip { get; }
    }
}
