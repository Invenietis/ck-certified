using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace CK.WindowManager.Model
{
    public sealed class WindowElement : IWindowElement
    {
        Window _w;
        string _name;

        public event EventHandler LocationChanged;

        public event EventHandler SizeChanged;

        public event EventHandler Hidden;

        public event EventHandler Restored;

        public WindowElement( Window w, string name )
        {
            if( w == null ) throw new ArgumentNullException( "w" );

            _name = name;
            _w = w;
            _w.LocationChanged += OnWindowLocationChanged;
            _w.SizeChanged += OnWindowSizeChanged;
        }

        public void Dispose()
        {
            _w.LocationChanged -= OnWindowLocationChanged;
            _w.SizeChanged -= OnWindowSizeChanged;
        }

        public Window Window
        {
            get { return _w; }
        }

        public Dispatcher Dispatcher
        {
            get { return _w.Dispatcher; }
        }

        void OnWindowSizeChanged( object sender, SizeChangedEventArgs e )
        {
            if( SizeChanged != null )
                SizeChanged( this, EventArgs.Empty );
        }

        public void OnWindowLocationChanged( object sender, EventArgs e )
        {
            if( LocationChanged != null )
                LocationChanged( this, e );
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
            using( _w.Dispatcher.DisableProcessing() )
            {
                _w.Top = top;
                _w.Left = left;
                Console.WriteLine( "{2} [{0},{1}]", _w.Top, _w.Left, _name );
            }
        }

        void IWindowElement.Resize( double width, double height )
        {
            using( _w.Dispatcher.DisableProcessing() )
            {
                _w.Width = width;
                _w.Height = height;
                Console.WriteLine( "{2} [{0},{1}]", _w.Width, _w.Height, _name );
            }
        }

    }
}
