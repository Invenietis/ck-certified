using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CK.Context;
using CK.Core;
using CK.Plugin;
using CK.WindowManager.Model;
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
        public IHostManipulator HostManipulator { get { return _hostManipulator ?? ( _hostManipulator = Context.ServiceContainer.GetService<IHostManipulator>() ); } }

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
            WindowElementData data = null;
            if( _dic.TryGetValue( window, out data ) )
            {
                WindowElementData cloneData = (WindowElementData)data.Clone();

                //Console.WriteLine( "WINDOW MANAGER Move ! {0} {1}*{2}", window.Name, top, left );
                window.Move( top, left );

                return new MoveResult( this, data, cloneData );
            }
            return NullResult.Default;
        }

        public virtual IManualInteractionResult Resize( IWindowElement window, double width, double height )
        {
            WindowElementData data = null;
            if( _dic.TryGetValue( window, out data ) )
            {
                WindowElementData cloneData = (WindowElementData)data.Clone();
                window.Resize( width, height );

                return new ResizeResult( this, data, cloneData );
            }
            return NullResult.Default;
        }

        protected virtual void OnWindowSizeChanged( object sender, EventArgs e )
        {
            IWindowElement windowElementFromSender = sender as IWindowElement;
            if( windowElementFromSender != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowElementFromSender, out data ) )
                {
                    double deltaWidth = data.Window.Width - data.Width;
                    double deltaHeight = data.Window.Height - data.Height;

                    data.UpdateFromWindow();

                    if( deltaWidth != 0 || deltaHeight != 0 )
                    {
                        var evt = new WindowElementResizeEventArgs( windowElementFromSender, deltaWidth, deltaHeight );
                        if( WindowResized != null )
                            WindowResized( sender, evt );
                    }
                }
            }
        }

        /// <summary>
        /// This function is called by a function that  bypass the window system event.
        /// Warning : do not call a function that is called when a WindowMoved event that leading to a call MoveResult.Broadcast
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnWindowLocationChanged( object sender, EventArgs e )
        {
            IWindowElement windowElementFromSender = sender as IWindowElement;
            if( windowElementFromSender != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowElementFromSender, out data ) )
                {
                    //Console.WriteLine( "OnWindowLocationChanged ! {0} {1}*{2}", data.Window.Name, data.Window.Top, data.Window.Left );
                    double deltaTop = data.Window.Top - data.Top;
                    double deltaLeft = data.Window.Left - data.Left;

                    data.UpdateFromWindow();

                    if( deltaTop != 0 || deltaLeft != 0 )
                    {
                        var evt = new WindowElementLocationEventArgs( windowElementFromSender, deltaTop, deltaLeft );
                        if( WindowMoved != null )
                            WindowMoved( sender, evt );
                    }
                }
            }
        }

        protected virtual void OnWindowMinimized( object sender, EventArgs e )
        {
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

        protected virtual void OnWindowRestored( object sender, EventArgs e )
        {
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

        protected virtual void OnWindowGotFocus( object sender, EventArgs e )
        {
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

            windowElement.GotFocus += OnWindowGotFocus;
            windowElement.Minimized += OnWindowMinimized;
            windowElement.Restored += OnWindowRestored;
            windowElement.LocationChanged += OnWindowLocationChanged;
            windowElement.SizeChanged += OnWindowSizeChanged;

            if( Registered != null )
                Registered( this, new WindowElementEventArgs( windowElement ) );
        }


        public virtual void Unregister( IWindowElement windowElement )
        {
            if( windowElement == null )
                throw new ArgumentNullException( "windowElement" );
            if( windowElement == null )
                throw new InvalidOperationException( "The window element holder must hold a valid, non null reference to a window element." );

            WindowElementData data = null;
            if( _dic.TryGetValue( windowElement, out data ) )
            {
                data.Window.GotFocus -= OnWindowGotFocus;
                data.Window.Minimized -= OnWindowMinimized;
                data.Window.Restored -= OnWindowRestored;
                data.Window.LocationChanged -= OnWindowLocationChanged;
                data.Window.SizeChanged -= OnWindowSizeChanged;
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
        /// Warning : create the reentrancy call if the same functio is call with WindowElement.LocationChanged event and WindowMoved event
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
            WindowElementData _clonedData;

            public MoveResult( WindowManager m, WindowElementData data, WindowElementData clonedData )
            {
                _m = m;
                _data = data;
                _clonedData = clonedData;

                data.Top = data.Window.Top;
                data.Left = data.Window.Left;
            }

            /// <summary>
            /// Propagate the homemade LocationChanged event.
            /// This function can create reentrancy problems because we bypass the Windows system event.
            /// </summary>
            public void Broadcast()
            {
                // Restores values...
                _data.Top = _clonedData.Top;
                _data.Left = _clonedData.Left;
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


        #region IWindowManager Members


        public void MinimizeAllWindows()
        {
            foreach( var w in _dic.Keys ) w.Minimize();
        }

        public void RestoreAllWindows()
        {
            foreach( var w in _dic.Keys ) w.Restore();
        }

        #endregion
    }
}
