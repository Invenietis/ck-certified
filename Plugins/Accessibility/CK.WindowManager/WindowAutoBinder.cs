using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CK.Plugin;
using CK.WindowManager.Model;
using CommonServices;
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

        public double AttractionRadius = 50;

        HitTester _tester;
        IBindResult _bindResult;

        public WindowAutoBinder()
        {
            _tester = new HitTester();
        }

        void OnWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            if( _tester.CanTest )
            {
                ISpatialBinding binding = WindowBinder.GetBinding( e.Window );
                IDictionary<IWindowElement, Rect> rect = WindowManager.WindowElements.ToDictionary( x => x, y => WindowManager.GetClientArea( y ) );

                IBinding result = _tester.Test( binding, rect, AttractionRadius );
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
                Point p =  new Point( e.X, e.Y );
                _window = WindowManager.WindowElements.FirstOrDefault( w => WindowManager.GetClientArea( w ).Contains( p ) );
                if( _window != null )
                {
                    _timer.Interval = CommonTimer.Service.Interval * 2;
                    _timer.AutoReset = true;
                    _timer.Start();
                    _timer.Elapsed += OnTimerElapsed;
                }
            }
        }

        void OnTimerElapsed( object sender, ElapsedEventArgs e )
        {
            if( _window != null )
            {
                var spatial = WindowBinder.GetBinding( _window );
                if( spatial.Top != null )
                {
                    WindowBinder.PreviewUnbind( _window, spatial.Top.Window );
                    WindowBinder.Unbind( _window, spatial.Top.Window );
                }
                if( spatial.Left != null )
                {
                    WindowBinder.PreviewUnbind( _window, spatial.Left.Window );
                    WindowBinder.Unbind( _window, spatial.Left.Window );
                }
                if( spatial.Right != null )
                {
                    WindowBinder.PreviewUnbind( _window, spatial.Right.Window );
                    WindowBinder.Unbind( _window, spatial.Right.Window );
                }
                if( spatial.Bottom != null )
                {
                    WindowBinder.PreviewUnbind( _window, spatial.Bottom.Window );
                    WindowBinder.Unbind( _window, spatial.Bottom.Window );
                }

                WindowManager.Move( _window, _window.Top + 20, _window.Left + 20 ).Silent();

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

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            _timer = new Timer();
            return true;
        }

        public void Start()
        {
            WindowBinder.BeforeBinding += OnBeforeBinding;
            WindowBinder.AfterBinding += OnAfterBinding;

            WindowManager.WindowMoved += OnWindowMoved;

            PointerDeviceDriver.PointerButtonDown += OnPointerButtonDown;
            PointerDeviceDriver.PointerMove += OnPointerMove;
            PointerDeviceDriver.PointerButtonUp += OnPointerButtonUp;

        }


        public void Stop()
        {
            PointerDeviceDriver.PointerButtonDown -= OnPointerButtonDown;
            PointerDeviceDriver.PointerMove -= OnPointerMove;
            PointerDeviceDriver.PointerButtonUp -= OnPointerButtonUp;

            WindowBinder.AfterBinding -= OnAfterBinding;
            WindowBinder.BeforeBinding -= OnBeforeBinding;

            WindowManager.WindowMoved -= OnWindowMoved;
        }

        public void Teardown()
        {
            _timer.Dispose();
        }

        #endregion
    }
}
