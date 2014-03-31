using System.Collections.Generic;
using System.Timers;
using CK.Plugin.Config;
using HighlightModel;

namespace Scroller
{
    internal interface IScrollingStrategy
    {
        bool IsStarted { get; }

        /// <summary>
        /// The displayable unique strategy name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Initialize the scrolling strategy with the given parameters
        /// </summary>
        /// <param name="timer">The heart beat timer</param>
        /// <param name="elements">the dictionnary of the registered elements</param>
        /// <param name="config">the config accessor</param>
        void Setup( Timer timer, Dictionary<string, IHighlightableElement> elements, IPluginConfigAccessor config );

        /// <summary>
        /// Start the scrolling strategy
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the scrolling strategy
        /// </summary>
        void Stop();

        /// <summary>
        /// This function forces the highlight of the given element.
        /// </summary>
        /// <param name="element">The element highlight.</param>
        /// <remarks> 
        /// This function is useful only when there is no alternative to.
        /// </remarks>
        void GoToElement( IHighlightableElement element );

        /// <summary>
        /// Suspend the scrolling strategy
        /// </summary>
        void Pause( bool forceEndHighlight );

        /// <summary>
        /// Resumes the scrillong strategy
        /// </summary>
        void Resume();

        /// <summary>
        /// Called after the trigger event is fired
        /// </summary>
        void OnExternalEvent();

        /// <summary>
        /// Warns the strategy that an element has been unregistered.
        /// Typically used to check whether the current element belongs to the unregistered element's tree, in order to call endhighlight on it.
        /// </summary>
        void ElementUnregistered( IHighlightableElement element );
    }
}
