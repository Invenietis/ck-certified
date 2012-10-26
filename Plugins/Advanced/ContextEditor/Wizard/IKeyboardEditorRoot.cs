using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Context;
using CK.Keyboard.Model;
using CK.Plugin;

namespace ContextEditor
{
    public interface IKeyboardEditorRoot : IKeyboardBackupManager
    {
        /// <summary>
        /// Gets the Keyboardcontext service implementation
        /// Gives access to all the keyboards and their layouts, zones, keys etc..
        /// </summary>
        IService<IKeyboardContext> KeyboardContext { get; }

        /// <summary>
        /// Gets the Context.
        /// Gives access to the application's inner management
        /// </summary>
        IContext Context { get; }
    }
}
