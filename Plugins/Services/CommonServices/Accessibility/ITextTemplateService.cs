using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CommonServices
{
    public interface ITextTemplateService : IDynamicService
    {
        /// <summary>
        /// The placeholder opent tag
        /// </summary>
        string OpentTag { get; }

        /// <summary>
        /// The placeholder close tag
        /// </summary>
        string CloseTag { get; }

        /// <summary>
        /// Open the window editor that allow the user to set
        /// the textbox fields
        /// </summary>
        void OpenEditor( string template );

        /// <summary>
        /// Send the result to the output
        /// </summary>
        void SendFormatedTemplate();
    }
}
