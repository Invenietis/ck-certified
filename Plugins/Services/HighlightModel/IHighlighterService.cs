using System;
using CK.Plugin;
using HighlightModel;

namespace CommonServices.Accessibility
{
    /// <summary>
    /// Service that allow other plugins to register themselves in order to be highlighted.
    /// </summary>
    public interface IHighlighterService : IDynamicService
    {
        /// <summary>
        /// Gets if the highlighter is running or not (maybe stopped, or just paused).
        /// </summary>
        bool IsHighlighting { get; }

        /// <summary>
        /// Register a highlightable tree in the service in order to be available for the highlighting.
        /// </summary>
        /// <param name="elementID"></param>
        /// <param name="root"></param>
        void RegisterTree( string elementID, IHighlightableElement root );

        /// <summary>
        /// Remove an tree that have been registered before. It can be a subtree of any highlightable element.
        /// </summary>
        /// <remarks>The elementID and the element must match the association made ​​in the collection</remarks>
        /// <param name="elementID"></param>
        /// <param name="element"></param>
        void UnregisterTree( string elementID, IHighlightableElement element );

        /// <summary>
        /// Adds an element at the beginning or end of an existing element that implements <see cref="IExtensibleHighlightableElement"/>.
        /// </summary>
        /// <param name="nameSpace">It is the elementID in which we will search  the <see cref="IExtensibleHighlightableElement"/> to add the element</param>
        /// <param name="extensibleElementName">It is the name of <see cref="IExtensibleHighlightableElement"/> in which we want to add the element</param>
        /// <param name="position">Pre or Post position of the element</param>
        /// <param name="element"></param>
        /// <returns>Return true, if the nameSpace contains the wanted <see cref="IExtensibleHighlightableElement"/> and if the <see cref="IExtensibleHighlightableElement"/> doesn't already contain the added element. Otherwise return false</returns>
        bool RegisterInRegisteredElementAt( string nameSpace, string extensibleElementName, ChildPosition position, IHighlightableElement element );

        /// <summary>
        /// Removes an element at the beginning or end of an existing element that implements <see cref="IExtensibleHighlightableElement"/>.
        /// </summary>
        /// <param name="nameSpace">It is the elementID in which we will search  the <see cref="IExtensibleHighlightableElement"/> to remove the element</param>
        /// <param name="extensibleElementName">It is the name of <see cref="IExtensibleHighlightableElement"/> in which we want to remove the element</param>
        /// <param name="position">Pre or Post position of the element</param>
        /// <param name="element"></param>
        /// <returns>Return true, if the nameSpace contains the wanted <see cref="IExtensibleHighlightableElement"/> and if the <see cref="IExtensibleHighlightableElement"/> contains the removed element. Otherwise return false</returns>
        bool UnregisterInRegisteredElement( string nameSpace, string extensibleElementName, ChildPosition position, IHighlightableElement element );

        /// <summary>
        /// Pause the highlighter scroller. Call Resume to resume the execution where it was paused.
        /// </summary>
        /// <param name="forceEndHighlight">If set to true, the highlighter will manually fire Endhighlight event to unselect the potentially highlighted current element</param>
        void Pause( bool forceEndHighlight = false );

        /// <summary>
        /// Resume the highlighter scroller.
        /// </summary>
        void Resume();

        ///// <summary>
        ///// Event fired to trigger the highlightment of a particular element (or tree).
        ///// </summary>
        //event EventHandler<HighlightEventArgs> BeginHighlight;

        ///// <summary>
        ///// Event fired to end the highlightment of a particular element (or tree).
        ///// </summary>
        //event EventHandler<HighlightEventArgs> EndHighlight;

        ///// <summary>
        ///// Event fired when an element has been spotted by the highlighter to be selected.
        ///// </summary>
        //event EventHandler<HighlightEventArgs> SelectElement;
    }

    public class HighlightEventArgs : EventArgs
    {
        public HighlightEventArgs( IHighlightableElement element )
        {
            Element = element;
        }

        public IHighlightableElement Element { get; private set; }
    }
}
