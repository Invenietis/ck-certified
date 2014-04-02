using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using CK.Context;
using CK.Core;
using CK.Plugin;
using CK.WindowManager.Model;
using CK.Windows;
using Host.Services;

namespace CK.WindowManager
{
    [Plugin( "{1B56170E-EB91-4E25-89B6-DEA94F85F604}", Categories = new string[] { "Accessibility" }, PublicName = "WindowManager", Version = "1.0.0" )]
    public class WindowManager : IWindowManager, IPlugin
    {
        IDictionary<IWindowElement, WindowElementData> _dic = new Dictionary<IWindowElement, WindowElementData>();

        IWindowElement _lastFocused;
        IHostManipulator _hostManipulator;

        [RequiredService]
        public IContext Context { get; set; }

        /// <summary>
        /// The HostManipulator, enables minimizing the host.
        /// </summary>
        public IHostManipulator HostManipulator { get { return _hostManipulator ?? (_hostManipulator = Context.ServiceContainer.GetService<IHostManipulator>()); } }

        public IReadOnlyList<IWindowElement> WindowElements
        {
            get { return _dic.Keys.ToArray(); }
        }

        public Rect GetClientArea( IWindowElement e )
        {
            WindowElementData d;
            _dic.TryGetValue( e, out d );
            if( d == null ) return Rect.Empty;

            return d.ToRect();
        }

        public virtual IManualInteractionResult Move( IWindowElement window, double top, double left )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

            WindowElementData data = null;
            if( _dic.TryGetValue( window, out data ) )
            {
                WindowElementData dataSnapshot = (WindowElementData)data.Clone();

                window.Move( top, left );

                return new MoveResult( this, data, dataSnapshot );
            }
            return NullResult.Default;
        }

        public virtual IManualInteractionResult Move( IWindowElement window, CallWithDelayedGet cwdg )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

            WindowElementData data = null;
            if( _dic.TryGetValue( window, out data ) )
            {
                WindowElementData dataSnapshot = (WindowElementData)data.Clone();

                window.Move( cwdg );

                return new MoveResult( this, data, dataSnapshot );
            }
            return NullResult.Default;
        }

        public virtual IManualInteractionResult Resize( IWindowElement window, double width, double height )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

            WindowElementData data = null;
            if( _dic.TryGetValue( window, out data ) )
            {
                WindowElementData cloneData = (WindowElementData)data.Clone();
                window.Resize( width, height );

                return new ResizeResult( this, data, cloneData );
            }
            return NullResult.Default;
        }

        public virtual IManualInteractionResult Resize( IWindowElement window, CallWithDelayedGet cwdg )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

            WindowElementData data = null;
            if( _dic.TryGetValue( window, out data ) )
            {
                WindowElementData cloneData = (WindowElementData)data.Clone();
                window.Resize( cwdg );

                return new ResizeResult( this, data, cloneData );
            }
            return NullResult.Default;
        }

        private void OnWindowSizeChangedInternal( object sender, EventArgs e )
        {
            DispatchWhenRequired( (Action)(() => OnWindowSizeChanged( sender, e )) );
        }

        protected virtual void OnWindowSizeChanged( object sender, EventArgs e )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

            IWindowElement windowElementFromSender = sender as IWindowElement;
            if( windowElementFromSender != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowElementFromSender, out data ) )
                {
                    double previousWidth = data.Width;
                    double previousHeight = data.Height;
                    data.UpdateFromWindow();
                    double deltaWidth = data.Width - previousWidth;
                    double deltaHeight = data.Height - previousHeight;

                    if( deltaWidth != 0 || deltaHeight != 0 )
                    {
                        var evt = new WindowElementResizeEventArgs( windowElementFromSender, deltaWidth, deltaHeight );
                        if( WindowResized != null )
                            WindowResized( sender, evt );
                    }
                }
            }
        }

        private void OnWindowLocationChangedInternal( object sender, EventArgs e )
        {
            DispatchWhenRequired( (Action)(() => OnWindowLocationChanged( sender, e )) );
        }

        /// <summary>
        /// This function is called by a function that bypasses the window system event.
        /// Warning : do not call a function that is called when a WindowMoved event that leading to a call MoveResult.Broadcast
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnWindowLocationChanged( object sender, EventArgs e )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread. Call OnWindowLocationChangedInternal to make sure the correct thread carries on." );

            IWindowElement windowElementFromSender = sender as IWindowElement;
            if( windowElementFromSender != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowElementFromSender, out data ) )
                {
                    //This is done to reduce the number of times we fetch a window's position directly from the Window, which could trigger Invokes
                    double previousTop = data.Top;
                    double previousLeft = data.Left;
                    data.UpdateFromWindow();
                    double deltaTop = data.Top - previousTop;
                    double deltaLeft = data.Left - previousLeft;

                    if( deltaTop != 0 || deltaLeft != 0 )
                    {
                        var evt = new WindowElementLocationEventArgs( windowElementFromSender, deltaTop, deltaLeft );
                        if( WindowMoved != null )
                            WindowMoved( sender, evt );
                    }
                }
            }
        }

        private void OnWindowMinimizedInternal( object sender, EventArgs e )
        {
            DispatchWhenRequired( (Action)(() => OnWindowMinimized( sender, e )) );
        }

        protected virtual void OnWindowMinimized( object sender, EventArgs e )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread. Call OnWindowMinimizedInternal to make sure the correct thread carries on." );

            IWindowElement windowElement = sender as IWindowElement;
            if( windowElement != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowElement, out data ) )
                {
                    if( WindowMinimized != null )
                        WindowMinimized( sender, new WindowElementEventArgs( windowElement ) );
                }
            }
        }

        private void OnWindowRestoredInternal( object sender, EventArgs e )
        {
            DispatchWhenRequired( (Action)(() => OnWindowRestored( sender, e )) );
        }

        protected virtual void OnWindowRestored( object sender, EventArgs e )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread. Call OnWindowRestoredInternal to make sure the correct thread carries on." );

            IWindowElement windowElement = sender as IWindowElement;
            if( windowElement != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowElement, out data ) )
                {
                    if( WindowRestored != null )
                        WindowRestored( sender, new WindowElementEventArgs( windowElement ) );
                }
            }
        }

        private void OnWindowGotFocusInternal( object sender, EventArgs e )
        {
            DispatchWhenRequired( (Action)(() => OnWindowGotFocus( sender, e )) );
        }

        protected virtual void OnWindowGotFocus( object sender, EventArgs e )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread. Call OnWindowGotFocus to make sure the correct thread carries on." );

            IWindowElement windowElement = sender as IWindowElement;
            if( windowElement != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowElement, out data ) )
                {
                    _lastFocused = windowElement;
                    if( WindowGotFocus != null )
                        WindowGotFocus( sender, new WindowElementEventArgs( windowElement ) );
                }
            }
        }

        public void ToggleHostMinimized()
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );
            IWindowElement element = _lastFocused;
            if( element == null && _dic.Count > 0 ) element = _dic.Keys.FirstOrDefault();

            if( element != null )
                element.ToggleHostMinimized( HostManipulator );
        }

        public IWindowElement GetByName( string name )
        {
            IWindowElement key = _dic.Keys.FirstOrDefault( x => x.Name == name );
            if( key != null ) return key;

            return null;
        }

        public virtual void Register( IWindowElement windowElement )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

            if( windowElement == null ) throw new ArgumentNullException( "windowElement" );
            if( _dic.ContainsKey( windowElement ) ) return;

            _dic.Add( windowElement, new WindowElementData
            {
                Window = windowElement,
                Height = windowElement.Height,
                Width = windowElement.Width,
                Left = windowElement.Left,
                Top = windowElement.Top
            } );

            windowElement.GotFocus += OnWindowGotFocusInternal;
            windowElement.Minimized += OnWindowMinimizedInternal;
            windowElement.Restored += OnWindowRestoredInternal;
            windowElement.LocationChanged += OnWindowLocationChangedInternal;
            windowElement.SizeChanged += OnWindowSizeChangedInternal;

            if( Registered != null )
                Registered( this, new WindowElementEventArgs( windowElement ) );
        }


        public virtual void Unregister( IWindowElement windowElement )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

            if( windowElement == null )
                throw new InvalidOperationException( "The window element holder must hold a valid, non null reference to a window element." );

            WindowElementData data = null;
            if( _dic.TryGetValue( windowElement, out data ) )
            {
                data.Window.GotFocus -= OnWindowGotFocusInternal;
                data.Window.Minimized -= OnWindowMinimizedInternal;
                data.Window.Restored -= OnWindowRestoredInternal;
                data.Window.LocationChanged -= OnWindowLocationChangedInternal;
                data.Window.SizeChanged -= OnWindowSizeChangedInternal;
                _dic.Remove( windowElement );

                if( Unregistered != null )
                    Unregistered( this, new WindowElementEventArgs( windowElement ) );
            }
        }

        #region Public Events

        public event EventHandler<WindowElementEventArgs> WindowGotFocus;

        public event EventHandler<WindowElementEventArgs> WindowMinimized;

        public event EventHandler<WindowElementEventArgs> WindowRestored;

        /// <summary>
        /// Warning : creates a reentrancy call if the same function is called with WindowElement.LocationChanged event and WindowMoved event
        /// </summary>
        public event EventHandler<WindowElementLocationEventArgs> WindowMoved;

        public event EventHandler<WindowElementResizeEventArgs> WindowResized;

        public event EventHandler<WindowElementEventArgs> Registered;

        public event EventHandler<WindowElementEventArgs> Unregistered;

        #endregion

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

        #region Action Result

        class NullResult : IManualInteractionResult
        {
            public static IManualInteractionResult Default = new NullResult();

            public void Broadcast()
            {
            }
            public void Silent()
            {
            }
        }

        class MoveResult : IManualInteractionResult
        {
            WindowManager _m;
            WindowElementData _data;
            WindowElementData _dataSnapshot;

            public MoveResult( WindowManager m, WindowElementData data, WindowElementData dataSnapshot )
            {
                _m = m;
                _data = data;
                _dataSnapshot = dataSnapshot;

                data.Top = data.Window.Top;
                data.Left = data.Window.Left;
            }

            /// <summary>
            /// Propagate the homemade LocationChanged event.
            /// This function can create reentrancy problems because we bypass the Windows system event.
            /// </summary>
            public void Broadcast()
            {
                Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );
                // Restores values...
                _data.Top = _dataSnapshot.Top;
                _data.Left = _dataSnapshot.Left;
                // Broadcast, with a homemade LocationChanged event
                _m.OnWindowLocationChanged( _data.Window, EventArgs.Empty );
            }

            public void Silent()
            {
            }
        }

        class ResizeResult : IManualInteractionResult
        {
            WindowManager _m;
            WindowElementData _data;
            WindowElementData _clonedData;

            public ResizeResult( WindowManager m, WindowElementData data, WindowElementData clonedData )
            {
                _m = m;
                _data = data;
                _clonedData = clonedData;

                data.Width = data.Window.Width;
                data.Height = data.Window.Height;
            }

            // <summary>
            /// Propagate the homemade WindowsSizeChanged event.
            /// This function can create reentrancy problems because we bypass the Windows system event.
            /// </summary>
            public void Broadcast()
            {
                // Restores values...
                _data.Width = _clonedData.Width;
                _data.Height = _clonedData.Height;
                // Broadcast, with a homemade WindowsSizeChanged event
                _m.OnWindowSizeChanged( _data.Window, EventArgs.Empty );
            }

            public void Silent()
            {
            }
        }

        #endregion

        class WindowElementData : ICloneable
        {
            public IWindowElement Window { get; set; }

            public double Top { get; set; }

            public double Left { get; set; }

            public double Width { get; set; }

            public double Height { get; set; }

            public object Clone()
            {
                return new WindowElementData { Window = this.Window, Top = this.Top, Left = this.Left, Width = this.Width, Height = this.Height };
            }

            public void UpdateFromWindow()
            {
                Top = Window.Top;
                Left = Window.Left;
                Width = Window.Width;
                Height = Window.Height;
            }

            internal Rect ToRect()
            {
                return new Rect( Left, Top, Width, Height );
            }
        }

        private void DispatchWhenRequired( Action a )
        {
            if( Application.Current.Dispatcher.CheckAccess() ) a();
            else Application.Current.Dispatcher.BeginInvoke( a );
        }

        #region IWindowManager Members

        public void MinimizeAllWindows()
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

            foreach( var w in _dic.Keys ) w.Minimize();
        }

        public void RestoreAllWindows()
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

            foreach( var w in _dic.Keys ) w.Restore();
        }

        #endregion
    }
}
