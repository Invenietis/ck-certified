using System;
using System.Collections.Generic;
using System.Windows;
using CK.Plugin;

namespace CK.WindowManager.Model
{
    /// <summary>
    /// A <see cref="IWindowElement"/> can register itself into the window manager. 
    /// The window manager knows location and size of each registered <see cref="IWindowElement"/> and provides
    /// events about their state (when they moved, they are resized, hidden or restored).
    /// </summary>
    public interface IWindowManager : IDynamicService
    {
        /// <summary>
        /// Gets all regsitered <see cref="IWindowElement"/>.
        /// </summary>
        IReadOnlyList<IWindowElement> WindowElements { get; }

        /// <summary>
        /// Gets a <see cref="IWindowElement"/> by name. Null if no window element are registered with this name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IWindowElement GetByName( string name );

        Rect GetClientArea( IWindowElement e );

        /// <summary>
        /// Registers the given window element
        /// </summary>
        /// <param name="window"></param>
        void Register( IWindowElement windowElement );

        /// <summary>
        /// Unregister the given window element
        /// </summary>
        /// <param name="window"></param>
        void Unregister( IWindowElement windowElement );

        /// <summary>
        /// Moves the given window element top the top and left.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="top"></param>
        /// <param name="left"></param>
        IManualInteractionResult Move( IWindowElement window, double top, double left );
        IManualInteractionResult Move( IWindowElement window, CallWithDelayedGet cwdg );

        /// <summary>
        /// Resizes the given window element to the width and height
        /// </summary>
        /// <param name="window"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        IManualInteractionResult Resize( IWindowElement window, double width, double height );
        IManualInteractionResult Resize( IWindowElement window, CallWithDelayedGet cwdg );

        void MinimizeAllWindows();

        void RestoreAllWindows();

        /// <summary>
        /// Minimizes the host.
        /// </summary>
        void ToggleHostMinimized();

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> got the focus.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementEventArgs> WindowGotFocus;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is registered.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementEventArgs> Registered;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is unregistered.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementEventArgs> Unregistered;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is hidden.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementEventArgs> WindowMinimized;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is restored.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementEventArgs> WindowRestored;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/>  is moved.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementLocationEventArgs> WindowMoved;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is resized.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementResizeEventArgs> WindowResized;

    }
}
