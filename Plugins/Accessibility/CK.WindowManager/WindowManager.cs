using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Plugin;
using CK.WindowManager.Model;

namespace CK.WindowManager
{
    [Plugin( "{1B56170E-EB91-4E25-89B6-DEA94F85F604}", Categories = new string[] { "Accessibility" }, PublicName = "WindowManager", Version = "1.0.0" )]
    public class WindowManager : IWindowManager, IPlugin
    {
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
        }

        IDictionary<IWindowElement, WindowElementData> _dic = new Dictionary<IWindowElement, WindowElementData>();

        public event EventHandler<WindowElementEventArgs> WindowHidden;

        public event EventHandler<WindowElementEventArgs> WindowRestored;

        public event EventHandler<WindowElementLocationEventArgs> WindowMoved;

        public event EventHandler<WindowElementResizeEventArgs> WindowResized;

        public event EventHandler<WindowElementEventArgs> Registered;

        public event EventHandler<WindowElementEventArgs> Unregistered;

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

            windowElement.Hidden += OnWindowHidden;
            windowElement.Restored += OnWindowRestored;
            windowElement.LocationChanged += OnWindowLocationChanged;
            windowElement.SizeChanged += OnWindowSizeChanged;

            if( Registered != null )
                Registered( this, new WindowElementEventArgs( windowElement ) );
        }

        class NullActionCallback : IActionCallback
        {
            public static IActionCallback Default = new NullActionCallback();

            public void BroadCast()
            {
            }
        }


        class MoveActionCallback : IActionCallback
        {
            WindowManager _m;
            WindowElementData _data;
            WindowElementData _clonedData;

            public MoveActionCallback( WindowManager m, WindowElementData data, WindowElementData clonedData )
            {
                _m = m;
                _data = data;
                _clonedData = clonedData;

                data.Top = _data.Window.Top;
                data.Left = _data.Window.Left;
            }

            public void BroadCast()
            {
                // Restores values...
                _data.Top = _clonedData.Top;
                _data.Left = _clonedData.Left;
                // Broadcast
                _m.OnWindowLocationChanged( _data.Window, EventArgs.Empty );
            }
        }

        class ResizeActionCallback : IActionCallback
        {
            WindowManager _m;
            IWindowElement _window;

            public ResizeActionCallback( WindowManager m, IWindowElement window )
            {
                _m = m;
                _window = window;
            }

            public void BroadCast()
            {
                _m.OnWindowSizeChanged( _window, EventArgs.Empty );
            }
        }

        public IActionCallback Move( IWindowElement window, double top, double left )
        {
            WindowElementData data = null;
            if( _dic.TryGetValue( window, out data ) )
            {
                WindowElementData cloneData = (WindowElementData)data.Clone();

                window.Move( top, left );

                return new MoveActionCallback( this, data, cloneData );
            }
            return NullActionCallback.Default;
        }

        public IActionCallback Resize( IWindowElement window, double width, double height )
        {
            WindowElementData data = null;
            if( _dic.TryGetValue( window, out data ) )
            {
                window.Resize( width, height );

                data.Width = window.Width;
                data.Height = window.Height;

                return new ResizeActionCallback( this, window );
            }
            return NullActionCallback.Default;
        }

        protected virtual void OnWindowSizeChanged( object sender, EventArgs e )
        {
            IWindowElement windowElementFromSender = sender as IWindowElement;
            if( windowElementFromSender != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowElementFromSender, out data ) )
                {
                    IWindowElement window = data.Window;
                    double deltaWidth = window.Width - data.Width;
                    double deltaHeight = window.Height - data.Height;

                    data.Width = window.Width;
                    data.Height = window.Height;

                    if( deltaWidth != 0 || deltaHeight != 0 )
                    {
                        var evt = new WindowElementResizeEventArgs( window, deltaWidth, deltaHeight );
                        if( WindowResized != null )
                            WindowResized( sender, evt );
                    }

                }
            }
        }

        protected virtual void OnWindowLocationChanged( object sender, EventArgs e )
        {
            IWindowElement windowElementFromSender = sender as IWindowElement;
            if( windowElementFromSender != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowElementFromSender, out data ) )
                {
                    IWindowElement window = data.Window;
                    double deltaTop = window.Top - data.Top;
                    double deltaLeft = window.Left - data.Left;

                    data.Top = window.Top;
                    data.Left = window.Left;
                    if( deltaTop != 0 || deltaLeft != 0 )
                    {
                        var evt = new WindowElementLocationEventArgs( windowElementFromSender, deltaTop, deltaLeft );
                        if( WindowMoved != null )
                            WindowMoved( sender, evt );
                    }
                }
            }
        }

        protected virtual void OnWindowHidden( object sender, EventArgs e )
        {
            IWindowElement windowElement = sender as IWindowElement;
            if( windowElement != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowElement, out data ) )
                {
                    if( WindowHidden != null )
                        WindowHidden( sender, new WindowElementEventArgs( windowElement ) );
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

        public virtual void Unregister( IWindowElement windowElement )
        {
            if( windowElement == null )
                throw new ArgumentNullException( "windowElement" );
            if( windowElement == null )
                throw new InvalidOperationException( "The window element holder must hold a valid, non null reference to a window element." );

            WindowElementData data = null;
            if( _dic.TryGetValue( windowElement, out data ) )
            {
                data.Window.Hidden -= OnWindowHidden;
                data.Window.Restored -= OnWindowRestored;
                data.Window.LocationChanged -= OnWindowLocationChanged;
                data.Window.SizeChanged -= OnWindowSizeChanged;
                _dic.Remove( windowElement );

                if( Unregistered != null )
                    Unregistered( this, new WindowElementEventArgs( windowElement ) );
            }
        }

        class DisableElementEvents : IDisposable
        {
            IReadOnlyCollection<IWindowElement> _holders;
            Action<IWindowElement> _eventToDisableTarget;
            Action<IWindowElement> _eventToEnableTarget;

            public DisableElementEvents(
                Action<IWindowElement> eventToDisableTarget,
                Action<IWindowElement> eventToEnableTarget,
                IEnumerable<IWindowElement> holders )
            {
                _eventToDisableTarget = eventToDisableTarget;
                _eventToEnableTarget = eventToEnableTarget;
                _holders = holders.ToArray();
                foreach( var h in _holders ) _eventToDisableTarget( h );
            }

            public void Dispose()
            {
                foreach( var h in _holders ) _eventToEnableTarget( h );
            }
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
