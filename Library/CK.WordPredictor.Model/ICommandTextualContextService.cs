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
    public interface ICommandTextualContextService : IDynamicService
    {
        /// <summary>
        /// This event is raised when the <see cref="ITextualContextService"/> has been clear.
        /// </summary>
        event EventHandler TextualContextClear;

        /// <summary>
        /// Raises the <see cref="TextualContextClear"/> event
        /// </summary>
        void ClearTextualContext();
    }
}
