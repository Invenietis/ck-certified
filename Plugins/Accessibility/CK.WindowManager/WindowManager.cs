using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.WindowManager.Model;

namespace CK.WindowManager
{
    public class WindowManager : IWindowManager, IPlugin
    {
        class WindowElementData
        {
            public IWindowElement Window { get; set; }

            public double Top { get; set; }

            public double Left { get; set; }

            public double Width { get; set; }

            public double Height { get; set; }
        }

        IDictionary<IWindowElement, WindowElementData> _dic = new Dictionary<IWindowElement, WindowElementData>();

        public event EventHandler<WindowElementEventArgs> WindowHidden;

        public event EventHandler<WindowElementEventArgs> WindowRestored;

        public event EventHandler<WindowElementLocationEventArgs> WindowMoved;

        public event EventHandler<WindowElementResizeEventArgs> WindowResized;

        public virtual void Register( IWindowElement window )
        {
            _dic.Add( window, new WindowElementData
            {
                Window = window,
                Height = window.Height,
                Width = window.Width,
                Left = window.Left,
                Top = window.Top
            } );

            window.Hidden += OnWindowHidden;
            window.Restored += OnWindowRestored;
            window.LocationChanged += OnWindowLocationChanged;
            window.SizeChanged += OnWindowSizeChanged;
        }

        protected virtual void OnWindowRestored( object sender, EventArgs e )
        {
            if( WindowRestored != null )
                WindowRestored( sender, new WindowElementEventArgs( sender as IWindowElement ) );
        }

        protected virtual void OnWindowSizeChanged( object sender, EventArgs e )
        {
            IWindowElement window = sender as IWindowElement;
            WindowElementData data = _dic[window];

            double deltaWidth = window.Width - data.Width;
            double deltaHeight = window.Height - data.Height;

            var evt = new WindowElementResizeEventArgs( window, deltaWidth, deltaHeight );
            if( WindowResized != null )
                WindowResized( sender, evt );

            data.Width = window.Width;
            data.Height = window.Height;
        }

        protected virtual void OnWindowLocationChanged( object sender, EventArgs e )
        {
            IWindowElement window = sender as IWindowElement;
            WindowElementData data = _dic[window];

            double deltaTop = window.Top - data.Top;
            double deltaLeft = window.Left - data.Left;

            var evt = new WindowElementLocationEventArgs( window, deltaTop, deltaLeft );
            if( WindowMoved != null )
                WindowMoved( sender, evt );

            data.Top = window.Top;
            data.Left = window.Left;
        }

        protected virtual void OnWindowHidden( object sender, EventArgs e )
        {
            if( WindowHidden != null )
                WindowHidden( sender, new WindowElementEventArgs( sender as IWindowElement ) );
        }

        public virtual void Unregister( IWindowElement window )
        {
            window.Hidden -= OnWindowHidden;
            window.Restored -= OnWindowRestored;
            window.LocationChanged -= OnWindowLocationChanged;
            window.SizeChanged -= OnWindowSizeChanged;
            _dic.Remove( window );
        }


        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
