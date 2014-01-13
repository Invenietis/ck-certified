using System;

namespace CK.WindowManager.Model
{
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

}
