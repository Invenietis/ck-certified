using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Gets a <see cref="IWindowElement"/> by name. Null if no window element are registered with this name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IWindowElement GetByName( string name );

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
        /// Raised when a <see cref="IWindowElement"/> is registered.
        /// </summary>
        event EventHandler<WindowElementEventArgs> Registered;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is unregistered.
        /// </summary>
        event EventHandler<WindowElementEventArgs> Unregistered;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is hidden.
        /// </summary>
        event EventHandler<WindowElementEventArgs> WindowHidden;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is restored.
        /// </summary>
        event EventHandler<WindowElementEventArgs> WindowRestored;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/>  is moved.
        /// </summary>
        event EventHandler<WindowElementLocationEventArgs> WindowMoved;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is resized.
        /// </summary>
        event EventHandler<WindowElementResizeEventArgs> WindowResized;
    }

}
