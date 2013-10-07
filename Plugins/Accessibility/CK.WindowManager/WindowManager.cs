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
            public IWindowElement2 Window { get; set; }

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
            IWindowElement key = _dic.Keys.FirstOrDefault( x => x.WindowElement.Name == name );
            if( key != null ) return key;

            return null;
        }

        public virtual void Register( IWindowElement windowHolder )
        {
            if( windowHolder == null ) throw new ArgumentNullException( "windowHolder" );
            if( windowHolder.WindowElement == null )
                throw new InvalidOperationException( "The window element holder must hold a valid, non null reference to a window element." );

            if( _dic.ContainsKey( windowHolder ) ) return;
            if( GetByName( windowHolder.WindowElement.Name ) != null ) return;

            _dic.Add( windowHolder, new WindowElementData
            {
                Window = windowHolder.WindowElement,
                Height = windowHolder.WindowElement.Height,
                Width = windowHolder.WindowElement.Width,
                Left = windowHolder.WindowElement.Left,
                Top = windowHolder.WindowElement.Top
            } );

            windowHolder.WindowElement.Hidden += OnWindowHidden;
            windowHolder.WindowElement.Restored += OnWindowRestored;
            windowHolder.WindowElement.LocationChanged += OnWindowLocationChanged;
            windowHolder.WindowElement.SizeChanged += OnWindowSizeChanged;

            if( Registered != null )
                Registered( this, new WindowElementEventArgs( windowHolder ) );
        }

        protected virtual void OnWindowRestored( object sender, EventArgs e )
        {
            IWindowElement windowHolder = sender as IWindowElement;
            if( windowHolder != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowHolder, out data ) )
                {
                    if( WindowRestored != null )
                        WindowRestored( sender, new WindowElementEventArgs( windowHolder ) );
                }
            }
        }

        protected virtual void OnWindowSizeChanged( object sender, EventArgs e )
        {
            IWindowElement windowHolder = sender as IWindowElement;
            if( windowHolder != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowHolder, out data ) )
                {
                    IWindowElement2 window = data.Window;
                    using( new DisableElementEvents( OnWindowSizeChanged, WindowBinder.GetAttachedElements( windowHolder ) ) )
                    {
                        double deltaWidth = window.Width - data.Width;
                        double deltaHeight = window.Height - data.Height;

                        var evt = new WindowElementResizeEventArgs( windowHolder, deltaWidth, deltaHeight );
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
            IWindowElement windowHolder = sender as IWindowElement;
            if( windowHolder != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowHolder, out data ) )
                {
                    IWindowElement2 window = data.Window;
                    using( new DisableElementEvents( OnWindowLocationChanged, WindowBinder.GetAttachedElements( windowHolder ) ) )
                    {
                        double deltaTop = window.Top - data.Top;
                        double deltaLeft = window.Left - data.Left;

                        var evt = new WindowElementLocationEventArgs( windowHolder, deltaTop, deltaLeft );
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
            IWindowElement windowHolder = sender as IWindowElement;
            if( windowHolder != null )
            {
                WindowElementData data = null;
                if( _dic.TryGetValue( windowHolder, out data ) )
                {
                    if( WindowHidden != null )
                        WindowHidden( sender, new WindowElementEventArgs( windowHolder ) );
                }
            }
        }

        public virtual void Unregister( IWindowElement windowHolder )
        {
            if( windowHolder == null )
                throw new ArgumentNullException( "windowHolder" );
            if( windowHolder.WindowElement == null )
                throw new InvalidOperationException( "The window element holder must hold a valid, non null reference to a window element." );

            WindowElementData data = null;
            if( _dic.TryGetValue( windowHolder, out data ) )
            {
                data.Window.Hidden -= OnWindowHidden;
                data.Window.Restored -= OnWindowRestored;
                data.Window.LocationChanged -= OnWindowLocationChanged;
                data.Window.SizeChanged -= OnWindowSizeChanged;
                _dic.Remove( windowHolder );

                if( Unregistered != null )
                    Unregistered( this, new WindowElementEventArgs( windowHolder ) );
            }
        }

        class DisableElementEvents : IDisposable
        {
            ICKReadOnlyCollection<IWindowElement> _holders;
            EventHandler _eventToDisable;

            public DisableElementEvents( EventHandler eventToDisable, ICKReadOnlyCollection<IWindowElement> holders )
            {
                _eventToDisable = eventToDisable;
                _holders = holders;
                foreach( var h in _holders ) h.WindowElement.LocationChanged -= _eventToDisable;
            }

            public void Dispose()
            {
                foreach( var h in _holders ) h.WindowElement.LocationChanged += _eventToDisable;
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
