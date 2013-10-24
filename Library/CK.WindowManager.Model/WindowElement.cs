﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using CK.Windows;
using Host.Services;

namespace CK.WindowManager.Model
{
    public sealed class WindowElement : IWindowElement
    {
        CKWindow _w;
        string _name;

        public event EventHandler LocationChanged;

        public event EventHandler SizeChanged;

        public event EventHandler Hidden;

        public event EventHandler Restored;

        public event EventHandler GotFocus;

        public WindowElement( CKWindow w, string name )
        {
            if( w == null ) throw new ArgumentNullException( "w" );

            _name = name;
            _w = w;
            _w.LocationChanged += OnWindowLocationChanged;
            _w.SizeChanged += OnWindowSizeChanged;
            _w.StateChanged += OnWindowStateChanged;
            _w.GotFocus += OnGotFocus;
        }

        void OnGotFocus( object sender, RoutedEventArgs e )
        {
            if( GotFocus != null )
            {
                GotFocus( this, e );
            }
        }

        void OnWindowStateChanged( object sender, EventArgs e )
        {
            if( _w.WindowState == WindowState.Minimized )
            {
                if( Hidden != null ) Hidden( this, EventArgs.Empty );
            }
            if( _w.WindowState == WindowState.Normal )
            {
                if( Restored != null ) Restored( this, EventArgs.Empty );
            }
        }

        public void Dispose()
        {
            _w.StateChanged -= OnWindowStateChanged;
            _w.LocationChanged -= OnWindowLocationChanged;
            _w.SizeChanged -= OnWindowSizeChanged;
            _w.GotFocus -= OnGotFocus;
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
            get { return DispatchWhenRequired( () => _w.Left ); }
        }

        double IWindowElement.Top
        {
            get { return DispatchWhenRequired( () => _w.Top ); }
        }

        double IWindowElement.Width
        {
            get { return DispatchWhenRequired( () => _w.Width ); }
        }

        double IWindowElement.Height
        {
            get { return DispatchWhenRequired( () => _w.Height ); }
        }

        void IWindowElement.Move( double top, double left )
        {
            DispatchWhenRequired( new Action( () =>
            {
                using( new DisableElementEvents( () => _w.LocationChanged -= OnWindowLocationChanged, () => _w.LocationChanged += OnWindowLocationChanged ) )
                {
                    if( top != _w.Top ) _w.Top = top;
                    if( left != _w.Left ) _w.Left = left;
                }
            } ) );
        }

        void IWindowElement.Resize( double width, double height )
        {
            DispatchWhenRequired( new Action( () =>
            {
                using( new DisableElementEvents( () => _w.SizeChanged -= OnWindowSizeChanged, () => _w.SizeChanged += OnWindowSizeChanged ) )
                {
                    _w.Width = width < 0 ? 0 : width;
                    _w.Height = height < 0 ? 0 : height;
                }
            } ) );
        }

        public void Hide()
        {
            DispatchWhenRequired( new Action( () => _w.Hide() ) );
        }

        public void Restore()
        {
            DispatchWhenRequired( new Action( () => _w.Show() ) );
        }

        private T DispatchWhenRequired<T>( Func<T> f )
        {
            if( Dispatcher.CheckAccess() ) return f();

            return (T)Dispatcher.Invoke( f );
        }

        private void DispatchWhenRequired( Action d )
        {
            if( Dispatcher.CheckAccess() ) d();
            else Dispatcher.BeginInvoke( d );
        }

        class DisableElementEvents : IDisposable
        {
            Action _eventToDisableTarget;
            Action _eventToEnableTarget;

            public DisableElementEvents(
                Action eventToDisableTarget,
                Action eventToEnableTarget )
            {
                _eventToDisableTarget = eventToDisableTarget;
                _eventToEnableTarget = eventToEnableTarget;
                _eventToDisableTarget();
            }

            public void Dispose()
            {
                _eventToEnableTarget();
            }
        }


        public void ToggleHostMinimized( IHostManipulator manipulator )
        {
            IntPtr ptr = IntPtr.Zero;
            Dispatcher.Invoke( (Action)(() => ptr = _w.Hwnd), null );
            Application.Current.Dispatcher.BeginInvoke( (Action)(() => manipulator.ToggleMinimize( ptr )), null );
        }
    }
}