using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightModel
{
    public interface IExtensibleHighlightableElement : IHighlightableElement
    {
        /// <summary>
        /// Allows the addition in PreChildren or PostChildren collections
        /// </summary>
        /// <param name="position"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        bool RegisterElementAt( ChildPosition position, IHighlightableElement child );

        /// <summary>
        /// Allows the deletion in PreChildren or PostChildren collections
        /// </summary>
        /// <param name="position"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        bool UnregisterElement( ChildPosition position, IHighlightableElement element );

        /// <summary>
        /// This is the name that identifies the IExtensibleHighlightableElement.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The list of virtual children with the <see cref="ChildPosition"/>.Pre. In front of the list of Children
        /// </summary>
        IReadOnlyList<IHighlightableElement> PreChildren { get; }

        /// <summary>
        /// The list of virtual children with the <see cref="ChildPosition"/>.Post. Behind the list of Children
        /// </summary>
        IReadOnlyList<IHighlightableElement> PostChildren { get; }
    }
}