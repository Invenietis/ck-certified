using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.WindowManager.Model
{
    public interface IBinding
    {
        IWindowElement First { get; }

        IWindowElement Second { get; }
    }

    public class WindowBindingEventArgs : EventArgs
    {
        IBinding Binding { get; set; }

        bool Canceled { get; set; }
    }

    public class WindowBindedEventArgs : EventArgs
    {
        IBinding Binding { get; set; }
    }

    public interface IWindowBinder
    {
        event EventHandler<WindowBindingEventArgs> BeforeBinding;

        event EventHandler<WindowBindedEventArgs> AfterBinding;

        ICollection<IBinding> AllBindings { get; }

    }

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

        double Top { get; }

        double Left { get; }

        double Width { get; }

        double Height { get; }

        event EventHandler LocationChanged;

        event EventHandler SizeChanged;

        event EventHandler Hidden;

        event EventHandler Restored;
    }

}
