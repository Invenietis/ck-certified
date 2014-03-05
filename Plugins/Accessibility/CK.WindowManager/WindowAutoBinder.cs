using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CK.Plugin;
using CK.WindowManager.Model;
using CommonServices;
using System.Timers;
using System;
using System.Diagnostics;
using SimpleSkin;

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

        const int XY_VARIATION_ACCEPTED = 5;

        Timer _timer = null;
        IWindowElement _window = null;
        Point _buttonDownPoint; //warning lifecycle, value type

        public double AttractionRadius = 65;

        HitTester _tester;
        IBindResult _bindResult;

        bool _resizeMoveLock; //avoids bind during a resize

        public WindowAutoBinder()
        {
            _tester = new HitTester();
        }

        void OnWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            //avoids bind during a resize
            if( !_resizeMoveLock )
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
            _resizeMoveLock = false;
            //Console.WriteLine( "OnWindowMoved ! {0} {1}*{2}", e.Window.Name, e.Window.Top, e.Window.Left );
        }

        void OnPointerButtonDown( object sender, PointerDeviceEventArgs e )
        {
            if( CommonTimer.Status.IsStartingOrStarted )
            {
                // Gets the window over the click
                _buttonDownPoint =  new Point( e.X, e.Y );
                _window = WindowManager.WindowElements.FirstOrDefault( w => WindowManager.GetClientArea( w ).Contains( _buttonDownPoint ) );
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
                    WindowBinder.Unbind( _window, spatial.Top.Window, false );
                }
                if( spatial.Left != null )
                {
                    WindowBinder.PreviewUnbind( _window, spatial.Left.Window );
                    WindowBinder.Unbind( _window, spatial.Left.Window, false );
                }
                if( spatial.Right != null )
                {
                    WindowBinder.PreviewUnbind( _window, spatial.Right.Window );
                    WindowBinder.Unbind( _window, spatial.Right.Window, false );
                }
                if( spatial.Bottom != null )
                {
                    WindowBinder.PreviewUnbind( _window, spatial.Bottom.Window );
                    WindowBinder.Unbind( _window, spatial.Bottom.Window, false );
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
                    if( (e.X < _buttonDownPoint.X + XY_VARIATION_ACCEPTED && e.X > _buttonDownPoint.X - XY_VARIATION_ACCEPTED)
                        && (e.Y < _buttonDownPoint.Y + XY_VARIATION_ACCEPTED && e.Y > _buttonDownPoint.Y - XY_VARIATION_ACCEPTED) )
                    {
                        _timer.Elapsed -= OnTimerElapsed;
                        _timer.Stop();
                        _window = null;
                    }
                }
            }
        }

        private Timer _activationTimer;

        //TODO test if the pointer is in the window
        private void OnPointerButtonUp( object sender, PointerDeviceEventArgs e )
        {
            //Allows the bypass the fact that Windows puts a window to the initial position
            //if the windows was moved during the PointerKeyUp treatment event
            if( _bindResult != null && _activationTimer == null )
            {
                //Console.WriteLine( "OnPointerButtonUp !" );
                _activationTimer = new Timer( 50 );
                _activationTimer.AutoReset = false;
                _activationTimer.Elapsed += t_Elapsed;
                _activationTimer.Start();
            }

            //stop the UnBind action when the button is up
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

        void t_Elapsed( object sender, ElapsedEventArgs e )
        {
            try
            {
                if( _bindResult != null )
                {
                    //Console.WriteLine( "Elapsed OnPointerButtonUp Seal !" );
                    _bindResult.Seal();
                }
            }
            finally
            {
                _bindResult = null;
                _activationTimer.Stop();
                _activationTimer.Dispose();
                _activationTimer = null;
            }
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
            PointerDeviceDriver.PointerButtonDown += OnPointerButtonDown;
            PointerDeviceDriver.PointerMove += OnPointerMove;
            PointerDeviceDriver.PointerButtonUp += OnPointerButtonUp;

            WindowBinder.BeforeBinding += OnBeforeBinding;
            WindowBinder.AfterBinding += OnAfterBinding;

            WindowManager.WindowMoved += OnWindowMoved;
            WindowManager.WindowResized += OnWindowResized;
        }

        //avoids bind during a resize
        void OnWindowResized( object sender, WindowElementResizeEventArgs e )
        {
            _resizeMoveLock = true;
        }


        public void Stop()
        {
            PointerDeviceDriver.PointerButtonDown -= OnPointerButtonDown;
            PointerDeviceDriver.PointerMove -= OnPointerMove;
            PointerDeviceDriver.PointerButtonUp -= OnPointerButtonUp;

            WindowBinder.AfterBinding -= OnAfterBinding;
            WindowBinder.BeforeBinding -= OnBeforeBinding;

            WindowManager.WindowMoved -= OnWindowMoved;
            WindowManager.WindowResized -= OnWindowResized;
        }

        public void Teardown()
        {
            _timer.Dispose();
        }

        #endregion
    }
}
