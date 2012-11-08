using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.WordPredictor.Model
{
    /// <summary>
    /// Sends the textual context service. 
    /// This is typically an action triggered by the user.
    /// </summary>
    public interface ISendTextualContextService : IDynamicService
    {
        /// <summary>
        /// This event is raised when the <see cref="ITextualContextService"/> has been sent by the service.
        /// </summary>
        event EventHandler TextualContextSent;

        /// <summary>
        /// Sends the 
        /// </summary>
        /// <param name="textualContext"></param>
        void SendTextualContext( ITextualContextService textualContext );
    }
}
