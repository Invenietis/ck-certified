﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CK.Plugin;
using CK.WindowManager.Model;
using CommonServices;
using System.Timers;
using System;
using System.Diagnostics;
using System.Windows.Threading;

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

        public double AttractionRadius = 65;

        HitTester _tester;
        IBindResult _bindResult;

        bool _resizeMoveLock; //avoids bind during a resize
        bool _pointerDownLock; //avoids bind during window.Move()

        public WindowAutoBinder()
        {
            _tester = new HitTester();
        }

        void OnWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            //avoids bind during a resize
            if( !_resizeMoveLock && !_pointerDownLock )
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

        private DispatcherTimer _activationTimer;

        //TODO test if the pointer is in the window
        private void OnPointerButtonUp( object sender, PointerDeviceEventArgs e )
        {
            //Allows the bypass the fact that Windows puts a window to the initial position
            //if the windows was moved during the PointerKeyUp treatment event
            if( _bindResult != null && _activationTimer == null )
            {
                _activationTimer = new DispatcherTimer();
                _activationTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
                _activationTimer.Tick += _activationTimer_Tick;
                _activationTimer.Start();
            }

            _pointerDownLock = true;
        }

        void _activationTimer_Tick( object sender, EventArgs e )
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
        
        //avoids bind during a resize
        void OnWindowResized( object sender, WindowElementResizeEventArgs e )
        {
            _resizeMoveLock = true;
        }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            _timer = new Timer();
            _pointerDownLock = true;
            return true;
        }

        public void Start()
        {
            PointerDeviceDriver.PointerButtonUp += OnPointerButtonUp;
            PointerDeviceDriver.PointerButtonDown += OnPointerButtonDown;

            WindowBinder.BeforeBinding += OnBeforeBinding;
            WindowBinder.AfterBinding += OnAfterBinding;

            WindowManager.WindowMoved += OnWindowMoved;
            WindowManager.WindowResized += OnWindowResized;
        }

        //avoids bind during window.Move()
        void OnPointerButtonDown( object sender, PointerDeviceEventArgs e )
        {
            _pointerDownLock = false;
        }

        public void Stop()
        {
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
