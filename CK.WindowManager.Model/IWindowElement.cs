using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.WindowManager.Model
{
    public enum BindingEventType
    {
        Attach,
        Detach
    }

    public class WindowBindingEventArgs : EventArgs
    {
        public BindingEventType BindingType { get; set; }

        public IBinding Binding { get; set; }

        public bool Canceled { get; set; }
    }

    public class WindowBindedEventArgs : EventArgs
    {
        public BindingEventType BindingType { get; set; }

        public IBinding Binding { get; set; }
    }

    public interface IWindowBinder
    {
        void Attach( IWindowElement first, IWindowElement second );

        void Detach( IBinding binding );

        event EventHandler<WindowBindingEventArgs> BeforeBinding;

        event EventHandler<WindowBindedEventArgs> AfterBinding;

        IList<IWindowElement> GetAttachedElements( IWindowElement referential );
    }

    public interface IBinding
    {
        IWindowElement First { get; }

        IWindowElement Second { get; }
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
        event EventHandler<WindowElementLocationEventArgs> WindowMoved;

        /// <summary>
        /// Raised when the window is moved.
        /// </summary>
        event EventHandler<WindowElementResizeEventArgs> WindowResized;
    }

    public class WindowElementEventArgs : EventArgs
    {
        public IWindowElement Window { get; private set; }

        public WindowElementEventArgs( IWindowElement window )
        {
            Window = window;
        }
    }

    public class WindowElementLocationEventArgs : WindowElementEventArgs
    {
        public double DeltaTop { get; private set; }

        public double DeltaLeft { get; private set; }

        public WindowElementLocationEventArgs( IWindowElement window, double deltaTop, double deltaLeft )
            : base( window )
        {
            DeltaTop = deltaTop;
            DeltaLeft = deltaLeft;
        }
    }

    public class WindowElementResizeEventArgs : WindowElementEventArgs
    {
        public double DeltaWidth { get; private set; }

        public double DeltaHeight { get; private set; }

        public WindowElementResizeEventArgs( IWindowElement window, double deltaWidth, double deltaHeight )
            : base( window )
        {
            DeltaWidth = deltaWidth;
            DeltaHeight = deltaHeight;
        }
    }

    public interface IWindowElement
    {
        string Name { get; }

        double Top { get; }

        double Left { get; }

        double Width { get; }

        double Height { get; }

        void Move( double top, double left );

        void Resize( double width, double height );

        event EventHandler LocationChanged;

        event EventHandler SizeChanged;

        event EventHandler Hidden;

        event EventHandler Restored;
    }

}
