using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightModel
{
    public interface IHighlightableElementController
    {
        /// <summary>
        /// Allows the parent to override the <see cref="ActionType"/> of children
        /// </summary>
        /// <param name="element">The child</param>
        /// <param name="action">ActionType send by child</param>
        /// <returns>The ActionType send by parent</returns>
        ActionType PreviewChildAction(IHighlightableElement element, ActionType action);

        /// <summary>
        /// Inform that a parent has override a <see cref="ActionType"/>
        /// </summary>
        /// <param name="action"></param>
        void OnChildAction( ActionType action );

        /// <summary>
        /// Inform that the element was unregistered in the tree.
        /// </summary>
        void OnUnregisterTree();
    }
}
