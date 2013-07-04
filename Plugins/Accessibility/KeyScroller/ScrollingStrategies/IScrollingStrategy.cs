using System;
using CommonServices.Accessibility;
using HighlightModel;

namespace KeyScroller
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IScrollingStrategy
    {
        /// <summary>
        /// The displayable unique strategy name
        /// </summary>
        string Name { get; }

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

        /// <summary>
        /// Start the scrolling strategy
        /// </summary>
        void Start();
        /// <summary>
        /// Stop the scrolling strategy
        /// </summary>
        void Stop();

        /// <summary>
        /// Suspend the scrolling strategy (Start() to resume)
        /// </summary>
        void Pause( bool forceEndHighlight );

        /// <summary>
        /// Called after the trigger event is fired
        /// </summary>
        void OnExternalEvent();

        /// <summary>
        /// A JLK Made method
        /// </summary>
        void ElementUnregistered( IHighlightableElement element );
    }
}
