using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CK.WindowManager.Model;

namespace CK.WindowManager
{
    public sealed class WindowElement : IWindowElement, IDisposable
    {
        Window _w;
        string _name;

        public IWindowManager WindowManager { get; private set; }

        public event EventHandler LocationChanged;

        public event EventHandler SizeChanged;

        public event EventHandler Hidden;

        public event EventHandler Restored;

        public WindowElement( IWindowManager m, Window w, string name )
        {
            if( m == null ) throw new ArgumentNullException( "m" );
            if( w == null ) throw new ArgumentNullException( "w" );

            _name = name;
            _w = w;
            _w.LocationChanged += OnWindowLocationChanged;
            _w.SizeChanged += OnWindowSizeChanged;
        
            WindowManager = m;
            WindowManager.Register( this );
        }

        public void Dispose()
        {
            WindowManager.Unregister( this );
            _w.LocationChanged -= OnWindowLocationChanged;
            _w.SizeChanged -= OnWindowSizeChanged;
        }

        public Window Window
        {
            get { return _w; }
        }

        void OnWindowSizeChanged( object sender, SizeChangedEventArgs e )
        {
            if( SizeChanged != null )
                SizeChanged( sender, EventArgs.Empty );
        }

        public void OnWindowLocationChanged( object sender, EventArgs e )
        {
            if( LocationChanged != null )
                LocationChanged( sender, e );
        }

        string IWindowElement.Name
        {
            get { return _name; }
        }

        double IWindowElement.Left
        {
            get { return _w.Left; }
        }

        double IWindowElement.Top
        {
            get { return _w.Top; }
        }

        double IWindowElement.Width
        {
            get { return _w.Width; }
        }

        double IWindowElement.Height
        {
            get { return _w.Height; }
        }

        void IWindowElement.Move( double top, double left )
        {
            _w.Top = top;
            _w.Left = left;
            Console.WriteLine( "{2} [{0},{1}]", _w.Top, _w.Left, _name );
        }

        void IWindowElement.Resize( double width, double height )
        {
            _w.Width = width;
            _w.Height = height;
        }

    }
}
