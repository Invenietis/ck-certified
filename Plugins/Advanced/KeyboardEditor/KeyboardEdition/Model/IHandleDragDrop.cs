using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyboardEditor.Model
{
    /// <summary>
    /// Interface that enables drag drop interactions between objects
    /// </summary>
    public interface IHandleDragDrop
    {
        /// <summary>
        /// Get or sets whether this element's drag drop behavior is enabled.
        /// </summary>
        bool IsDragDropEnabled { get; set; }

        /// <summary>
        /// Gets whether this element accepts to have the specified drop source dropped on it.
        /// </summary>
        /// <param name="dropSource">The element that should be dropped</param>
        /// <returns>True if the element accepts to be the target of the to-be-dropped <see cref="IHandleDragDrop"/> element. False otherwise.</returns>
        bool CanBeDropTarget( IHandleDragDrop dropSource );

        /// <summary>
        /// Gets whether this element accepts being dropped on the specified target.
        /// </summary>
        /// <param name="dropTarget">The drop target <see cref="IHandleDragDrop"/> element.</param>
        /// <returns>True if this element accepts to be dropped on the specified target. False otherwise.</returns>
        bool CanBeDropSource( IHandleDragDrop dropTarget );

        /// <summary>
        /// The actual drop action. Should be called on the TARGET, with the SOURCE as parameter.
        /// </summary>
        /// <param name="dropSource">The drop source. (the <see cref="IHandleDragDrop"/> element to be dropped).</param>
        void ExecuteDropAction( IHandleDragDrop dropSource );
    }

}
