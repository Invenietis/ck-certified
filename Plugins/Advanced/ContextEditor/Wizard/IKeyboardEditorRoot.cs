using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Context;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;

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

        /// <summary>
        /// Gets the <see cref="IPluginConfigAccessor"/> linked ot the configuration of the skin plugin
        /// </summary>
        IPluginConfigAccessor SkinConfiguration { get; }

        /// <summary>
        /// Gets the <see cref="IPluginConfigAccessor"/> linked ot the configuration of the keyboard editor
        /// </summary>
        IPluginConfigAccessor Config { get; }

        /// <summary>
        /// Gets the PointerDeviceDriver service , is used to hook mouse events to enable drag n drop for keys
        /// </summary>
        IService<IPointerDeviceDriver> PointerDeviceDriver { get; }
    }
}
