using System;
using System.Windows;
using Host.Services;

namespace CK.WindowManager.Model
{
    public interface IWindowElement : IDisposable
    {
        /// <summary>
        /// Raised when the window element is focused.
        /// </summary>
        event EventHandler GotFocus;

        /// <summary>
        /// Raised when the window element location changed.
        /// </summary>
        event EventHandler LocationChanged;

        /// <summary>
        /// Raised when the window element size changed.
        /// </summary>
        event EventHandler SizeChanged;

        /// <summary>
        /// Raised when the window element is hidden.
        /// </summary>
        event EventHandler Minimized;

        /// <summary>
        /// Raised when the window element is restored (from hidden state for ex).
        /// </summary>
        event EventHandler Restored;
        
        /// <summary>
        /// Gets the name of the window element.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets top edge, in relation to the desktop, in logical units (1/96").
        /// If called from another thread and in order to get this info without fearing deadlocks, try <see cref="IWindowManager.GetClientArea"/> 
        /// </summary>
        double Top { get; }

        /// <summary>
        /// Gets the left edge, in relation to the desktop, in logical units (1/96").
        /// If called from another thread and in order to get this info without fearing deadlocks, try <see cref="IWindowManager.GetClientArea"/> 
        /// </summary>
        double Left { get; }

        /// <summary>
        /// Gets the width, in device-independent units (1/96").
        /// If called from another thread and in order to get this info without fearing deadlocks, try <see cref="IWindowManager.GetClientArea"/> 
        /// </summary>
        double Width { get; }

        /// <summary>
        /// Gets the height, in device-independent units (1/96").
        /// If called from another thread and in order to get this info without fearing deadlocks, try <see cref="IWindowManager.GetClientArea"/> 
        /// </summary>
        double Height { get; }

        /// <summary>
        /// Moves the window element at the given top and left in relation to the desktop.
        /// </summary>
        /// <param name="top">The top in logical units (1/96")</param>
        /// <param name="left">The left in logical units (1/96")</param>
        void Move( double top, double left );

        /// <summary>
        /// Resizes the window element to the given width and height
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void Resize( double width, double height );

        void Minimize();

        void Restore();

        void ToggleHostMinimized( IHostManipulator manipulator );

        Window Window { get; }
    }

}
