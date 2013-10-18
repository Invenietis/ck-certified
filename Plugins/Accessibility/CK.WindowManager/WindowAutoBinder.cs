using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CK.Plugin;
using CK.WindowManager.Model;
using CommonServices;
using System.Diagnostics;
using System.Timers;

namespace CK.WindowManager
{
    [Plugin( "{B63BB144-1C13-4A3B-93BD-AC5233F4F18E}", PublicName = "CK.WindowManager.AutoBinder" )]
    public class WindowAutoBinder : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowManager WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowBinder WindowBinder { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IPointerDeviceDriver PointerDeviceDriver { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ICommonTimer> CommonTimer { get; set; }

        Timer _timer = null;
        IWindowElement _window = null;
        IDictionary<IWindowElement, Rect> _rect;

        public double AttractionRadius = 50;

        HitTester _tester;
        IBindResult _bindResult;

        public WindowAutoBinder()
        {
            _tester = new HitTester();
        }

        void OnWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            _rect[e.Window] = new Rect( e.Window.Left, e.Window.Top, e.Window.Width, e.Window.Height );

            if( _tester.CanTest )
            {
                ISpatialBinding binding = WindowBinder.GetBinding( e.Window );
                IReadOnlyList<IWindowElement> registeredElements = WindowManager.WindowElements;

                IBinding result = _tester.Test( binding, _rect, AttractionRadius );
                if( result != null )
                {
                    _bindResult = WindowBinder.PreviewBind( result.Target, result.Origin, result.Position );
                }
                else
                {
                    if( _tester.LastResult != null )
                    {
                        WindowBinder.PreviewUnbind( _tester.LastResult.Target, _tester.LastResult.Origin );
                        _bindResult = null;
                    }
                }
            }
        }


        void OnPointerButtonDown( object sender, PointerDeviceEventArgs e )
        {
            if( CommonTimer.Status.IsStartingOrStarted )
            {
                // Gets the window over the click
                foreach( var r in _rect )
                {
                    if( r.Value.Contains( new Point( e.X, e.Y ) ) )
                    {
                        _window = r.Key;
                    }
                }
                if( _window != null )
                {
                    //_timer.Interval = CommonTimer.Service.Interval;
                    //_timer.AutoReset = true;
                    //_timer.Start();
                    //_timer.Elapsed += OnTimerElapsed;
                }
            }
        }

        void OnTimerElapsed( object sender, ElapsedEventArgs e )
        {
            if( _window != null )
            {
                var spatial = WindowBinder.GetBinding( _window );
                
                if( spatial.Top != null ) WindowBinder.PreviewUnbind( _window, spatial.Top.Window );
                if( spatial.Left != null ) WindowBinder.PreviewUnbind( _window, spatial.Left.Window );
                if( spatial.Right != null ) WindowBinder.PreviewUnbind( _window, spatial.Right.Window );
                if( spatial.Bottom != null ) WindowBinder.PreviewUnbind( _window, spatial.Bottom.Window  );

                _timer.Elapsed -= OnTimerElapsed;
                _timer.Stop();
                _window = null;
            }
        }

        void OnPointerMove( object sender, PointerDeviceEventArgs e )
        {
            if( CommonTimer.Status.IsStartingOrStarted )
            {
                if( _timer.Enabled )
                {
                    _timer.Elapsed -= OnTimerElapsed;
                    _timer.Stop();
                    _window = null;
                }
            }
        }

        private void OnPointerButtonUp( object sender, PointerDeviceEventArgs e )
        {
            if( _bindResult != null ) _bindResult.Seal();
        }

        void OnBeforeBinding( object sender, WindowBindingEventArgs e )
        {
            _tester.Block();
        }

        void OnAfterBinding( object sender, WindowBindedEventArgs e )
        {
            _tester.Release();
        }

        void OnWindowRegistered( object sender, WindowElementEventArgs e )
        {
            RegisterWindow( e.Window );
        }

        void OnWindowuUregistered( object sender, WindowElementEventArgs e )
        {
            UnegisterWindow( e.Window );
        }

        private void RegisterWindow( IWindowElement window )
        {
            if( !_rect.ContainsKey( window ) )
            {
                var rect = new Rect( window.Left, window.Top, window.Width, window.Height );
                _rect.Add( window, rect );
            }
        }

        private void UnegisterWindow( IWindowElement windowElement )
        {
            _rect.Remove( windowElement );
        }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            _timer = new Timer();
            _rect = new Dictionary<IWindowElement, Rect>();
            return true;
        }

        public void Start()
        {
            WindowBinder.BeforeBinding += OnBeforeBinding;
            WindowBinder.AfterBinding += OnAfterBinding;

            WindowManager.Registered += OnWindowRegistered;
            WindowManager.Unregistered += OnWindowuUregistered;

            WindowManager.WindowMoved += OnWindowMoved;

            PointerDeviceDriver.PointerButtonDown += OnPointerButtonDown;
            PointerDeviceDriver.PointerMove += OnPointerMove;
            PointerDeviceDriver.PointerButtonUp += OnPointerButtonUp;

            foreach( IWindowElement e in WindowManager.WindowElements ) RegisterWindow( e );
        }

        public void Stop()
        {
            _rect.Clear();

            PointerDeviceDriver.PointerButtonDown -= OnPointerButtonDown;
            PointerDeviceDriver.PointerMove -= OnPointerMove;
            PointerDeviceDriver.PointerButtonUp -= OnPointerButtonUp;

            WindowBinder.AfterBinding -= OnAfterBinding;
            WindowBinder.BeforeBinding -= OnBeforeBinding;

            WindowManager.Registered -= OnWindowRegistered;
            WindowManager.Unregistered -= OnWindowuUregistered;

            WindowManager.WindowMoved -= OnWindowMoved;
        }

        public void Teardown()
        {
            _timer.Dispose();
        }

        #endregion
    }
}
