#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WindowManager.Model\WindowElement.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Windows;
using System.Windows.Threading;
using CK.Windows;
using Host.Services;

namespace CK.WindowManager.Model
{
    public sealed class WindowElement : IWindowElement
    {
        readonly CKWindow _w;
        readonly string _name;

        public event EventHandler LocationChanged;

        public event EventHandler SizeChanged;

        public event EventHandler Minimized;

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
                if( Minimized != null ) Minimized( this, EventArgs.Empty );
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
            DispatchWhenRequired( () =>
            {
                using( new DisableElementEvents( () => _w.LocationChanged -= OnWindowLocationChanged, () => _w.LocationChanged += OnWindowLocationChanged ) )
                {
                    if( top != _w.Top ) _w.Top = top;
                    if( left != _w.Left ) _w.Left = left;
                }
            } );
        }

        void IWindowElement.Resize( double width, double height )
        {
            DispatchWhenRequired( () =>
            {
                using( new DisableElementEvents( () => _w.SizeChanged -= OnWindowSizeChanged, () => _w.SizeChanged += OnWindowSizeChanged ) )
                {
                    if( _w.Width != width ) _w.Width = width < 0 ? 0 : width;
                    if( _w.Height !=height ) _w.Height = height < 0 ? 0 : height;
                }
            } );
        }

        public void Minimize()
        {
            DispatchWhenRequired( () => _w.WindowState = WindowState.Minimized, false );
        }

        public void Restore()
        {
            DispatchWhenRequired( () => _w.WindowState = WindowState.Normal, false );
        }

        public void Hide()
        {
            DispatchWhenRequired( () => _w.Hide(), false );
        }

        public void Show()
        {
            DispatchWhenRequired( () => _w.Show(), false );
        }

        public void Close()
        {
            DispatchWhenRequired( () => _w.Close(), false );
        }

        private T DispatchWhenRequired<T>( Func<T> f )
        {
            if( Dispatcher.CheckAccess() ) return f();

            return (T)Dispatcher.Invoke( f );
        }

        private void DispatchWhenRequired( Action d, bool synchronous = true )
        {
            if( Dispatcher.CheckAccess() ) d();
            else
            {
                if( synchronous )
                    Dispatcher.Invoke( d );
                else
                    Dispatcher.BeginInvoke( d );
            }
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
            NoFocusManager.Default.ExternalDispatcher.BeginInvoke( (Action)(() => manipulator.ToggleMinimize( ptr )), null );
        }
    }
}
