using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.WindowManager.Model
{
    /// <summary>
    /// Defines what can be bone after a manual interaction on the WindowManager.
    /// </summary>
    public interface IManualInteractionResult : CK.Core.IFluentInterface
    {
        /// <summary>
        /// Does not propagate the event. This is the default behavior.
        /// </summary>
        void Silent();
        /// <summary>
        /// Broadcast the event resulting of the manual interaction.
        /// </summary>
        void Broadcast();
    }
}
