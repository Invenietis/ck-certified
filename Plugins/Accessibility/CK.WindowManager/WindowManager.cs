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
        IDictionary<IWindowElement, WindowElementData> _dic = new Dictionary<IWindowElement, WindowElementData>();

        public virtual IManualInteractionResult Move( IWindowElement window, double top, double left )
        {
            WindowElementData data = null;
            if( _dic.TryGetValue( window, out data ) )
            {
                WindowElementData cloneData = (WindowElementData)data.Clone();

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

        protected virtual void OnWindowLocationChanged( object sender, EventArgs e )
        {
            IWindowElement windowElementFromSender = sender as IWindowElement;
            if( windowElementFromSender != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowElementFromSender, out data ) )
                {
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

        #region Public Events

        public event EventHandler<WindowElementEventArgs> WindowHidden;

        public event EventHandler<WindowElementEventArgs> WindowRestored;

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

            public void Broadcast()
            {
                // Restores values...
                _data.Top = _clonedData.Top;
                _data.Left = _clonedData.Left;
                // Broadcast
                _m.OnWindowLocationChanged( _data.Window, EventArgs.Empty );
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

            public void Broadcast()
            {
                // Restores values...
                _data.Width = _clonedData.Width;
                _data.Height = _clonedData.Height;
                // Broadcast
                _m.OnWindowSizeChanged( _data.Window, EventArgs.Empty );
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
        }
    }
}
