using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CK.Plugin;
using CK.WindowManager.Model;
using CommonServices;
using System.Diagnostics;

namespace CK.WindowManager
{
    [Plugin( "{B63BB144-1C13-4A3B-93BD-AC5233F4F18E}", PublicName = "CK.WindowManager.AutoBinder" )]
    public class WindowAutoBinder : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IPointerDeviceDriver PointerDeviceDriver { get; set; }

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
                ISpatialBinding binding = WindowBinder.Service.GetBinding( e.Window );
                IReadOnlyList<IWindowElement> registeredElements = WindowManager.Service.WindowElements;

                IBinding result = _tester.Test( binding, _rect, AttractionRadius );
                if( result != null )
                {
                    _bindResult = WindowBinder.Service.PreviewBind( result.Master, result.Slave, result.Position );
                }
                else
                {
                    if( _tester.LastResult != null )
                        _bindResult = WindowBinder.Service.PreviewUnbind( _tester.LastResult.Master, _tester.LastResult.Slave );
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
            _rect = new Dictionary<IWindowElement, Rect>();
            return true;
        }

        public void Start()
        {
            WindowBinder.Service.BeforeBinding += OnBeforeBinding;
            WindowBinder.Service.AfterBinding += OnAfterBinding;

            WindowManager.Service.Registered += OnWindowRegistered;
            WindowManager.Service.Unregistered += OnWindowuUregistered;

            WindowManager.Service.WindowMoved += OnWindowMoved;

            PointerDeviceDriver.PointerButtonUp += OnPointerButtonUp;

            foreach( IWindowElement e in WindowManager.Service.WindowElements ) RegisterWindow( e );
        }

        public void Stop()
        {
            _rect.Clear();

            PointerDeviceDriver.PointerButtonUp -= OnPointerButtonUp;

            WindowBinder.Service.AfterBinding -= OnAfterBinding;
            WindowBinder.Service.BeforeBinding -= OnBeforeBinding;

            WindowManager.Service.Registered -= OnWindowRegistered;
            WindowManager.Service.Unregistered -= OnWindowuUregistered;

            WindowManager.Service.WindowMoved -= OnWindowMoved;
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
