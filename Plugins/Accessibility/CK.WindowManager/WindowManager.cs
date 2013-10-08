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
        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowBinder WindowBinder { get; set; }

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

        protected virtual void OnWindowSizeChanged( object sender, EventArgs e )
        {
            IWindowElement windowElementFromSender = sender as IWindowElement;
            if( windowElementFromSender != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowElementFromSender, out data ) )
                {
                    IWindowElement window = data.Window;
                    using( new DisableElementEvents( 
                        w => w.SizeChanged -= OnWindowSizeChanged,
                        w => w.SizeChanged += OnWindowSizeChanged,
                        WindowBinder.GetBinding( window ) ) )
                    {
                        double deltaWidth = window.Width - data.Width;
                        double deltaHeight = window.Height - data.Height;

                        var evt = new WindowElementResizeEventArgs( window, deltaWidth, deltaHeight );
                        if( WindowResized != null )
                            WindowResized( sender, evt );

                        data.Width = window.Width;
                        data.Height = window.Height;
                    }

                }
            }
        }

        protected virtual void OnWindowLocationChanged( object sender, EventArgs e )
        {
            IWindowElement windowElement = sender as IWindowElement;
            if( windowElement != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowElement, out data ) )
                {
                    IWindowElement window = data.Window;
                    using( new DisableElementEvents(
                        w => w.LocationChanged -= OnWindowLocationChanged,
                        w => w.LocationChanged += OnWindowLocationChanged,
                        WindowBinder.GetBinding( windowElement ) ) )
                    {
                        double deltaTop = window.Top - data.Top;
                        double deltaLeft = window.Left - data.Left;

                        var evt = new WindowElementLocationEventArgs( windowElement, deltaTop, deltaLeft );
                        if( WindowMoved != null )
                            WindowMoved( sender, evt );

                        data.Top = window.Top;
                        data.Left = window.Left;
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
            ICKReadOnlyCollection<IWindowElement> _holders;
            Action<IWindowElement> _eventToDisableTarget;
            Action<IWindowElement> _eventToEnableTarget;

            public DisableElementEvents(
                Action<IWindowElement> eventToDisableTarget,
                Action<IWindowElement> eventToEnableTarget,
                ICKReadOnlyCollection<IWindowElement> holders )
            {
                _eventToDisableTarget = eventToDisableTarget;
                _eventToEnableTarget = eventToEnableTarget;
                _holders = holders;
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
