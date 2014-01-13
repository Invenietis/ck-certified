using CK.Context;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;
using KeyboardEditor.ViewModels;
using ProtocolManagerModel;

namespace KeyboardEditor
{
    public interface IKeyboardEditorRoot : IKeyboardBackupManager
    {
        /// <summary>
        /// Gets the Keyboardcontext service implementation
        /// Gives access to all the keyboards and their layouts, zones, keys etc..
        /// </summary>
        IService<IKeyboardContext> KeyboardContext { get; }

        /// <summary>
        /// Gets the Keyboardcontext service implementation
        /// Gives access to all the keyboards and their layouts, zones, keys etc..
        /// </summary>
        IService<IProtocolEditorsManager> ProtocolManagerService { get; }

        /// <summary>
        /// Gets a service that enables hooking windows' low level keyboard inputs
        /// </summary>
        //IService<IKeyboardDriver> KeyboardDriver { get; }

        event KeyboardEditor.HookInvokedEventHandler HookInvoqued;

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

        /// <summary>
        /// Gets the Viewmodels of the keyboard that is being edited.
        /// Used to trigger Dispose on viewmodels when stopping the plugin
        /// </summary>
        VMContextEditable EditedContext { get; set; }

        void ShowHelp();
    }
}
