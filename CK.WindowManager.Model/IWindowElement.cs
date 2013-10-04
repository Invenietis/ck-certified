using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.WindowManager.Model
{
    public interface IWindowManager
    {
        /// <summary>
        /// Registers the given window element
        /// </summary>
        /// <param name="window"></param>
        void Register( IWindowElement window );

        /// <summary>
        /// Unregister the given window element
        /// </summary>
        /// <param name="window"></param>
        void Unregister( IWindowElement window );

        /// <summary>
        /// Moves the given window element to the given position
        /// </summary>
        /// <param name="window"></param>
        /// <param name="descriptor"></param>
        void Move( IWindowElement window, int x, int y );

        /// <summary>
        /// Resizes the given window element
        /// </summary>
        /// <param name="window"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void Resize( IWindowElement window, int width, int height );

        /// <summary>
        /// Hides the window.
        /// </summary>
        void Hide( IWindowElement window );

        /// <summary>
        /// Restores the window.
        /// </summary>
        void Restore( IWindowElement window );

        /// <summary>
        /// Toggles minimization of the host's window.
        /// </summary>
        void ToggleHostMinimized( IWindowElement window );

        /// <summary>
        /// Raised when the window is hidden.
        /// </summary>
        event EventHandler<WindowElementEventArgs> WindowHidden;

        /// <summary>
        /// Raised when the window is restored.
        /// </summary>
        event EventHandler<WindowElementEventArgs> WindowRestored;

        /// <summary>
        /// Raised when the window is moved.
        /// </summary>
        event EventHandler<WindowElementEventArgs> WindowMoved;

        /// <summary>
        /// Raised when the window is moved.
        /// </summary>
        event EventHandler<WindowElementEventArgs> WindowResized;
    }

    public class WindowElementEventArgs : EventArgs
    {
        public IWindowElement Window { get; private set; }

        public WindowElementEventArgs( IWindowElement window )
        {
            Window = window;
        }
    }

    public interface IWindowElement
    {
        string Name { get; }

        int X { get; }

        int Y { get; }

        int Width { get; }

        int Height { get; }

        ICollection<IBinder> Binders { get; }
    }

    public interface IBinder
    {

    }
}
