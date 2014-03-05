using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonServices
{
    public interface ITextTemplateCommandHandlerService
    {
        /// <summary>
        /// Open the window editor that allow the user to set
        /// the textbox fields and to send the result to the output
        /// </summary>
        void OpenEditor(string template);
    }
}
