using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Register a highlightable tree in the service in order to be available for the highlighting
        /// </summary>
        /// <param name="root"></param>
        void RegisterTree( IHighlightableElement root );

        /// <summary>
        /// Remove an tree that have been registered before. It can be a subtree of any highlightable element.
        /// </summary>
        /// <param name="element"></param>
        void UnregisterTree( IHighlightableElement element );

        /// <summary>
        /// Event fired to trigger the highlightment of a particular element (or tree).
        /// </summary>
        event EventHandler<HighlightEventArgs> BeginHighlight;

        /// <summary>
        /// Event fired to end the highlightment of a particular element (or tree).
        /// </summary>
        event EventHandler<HighlightEventArgs> EndHighlight;

        /// <summary>
        /// Event fired when an element has been spotted by the highlighter to be selected.
        /// </summary>
        event EventHandler<HighlightEventArgs> SelectElement;
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
