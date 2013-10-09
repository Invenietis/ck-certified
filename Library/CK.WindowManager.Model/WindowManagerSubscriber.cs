using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CK.Plugin;

namespace CK.WindowManager.Model
{
    public class WindowManagerSubscriber
    {
        string _name;
        Window _window;
        readonly IService<IWindowManager> _windowManager;
        readonly IService<IWindowBinder> _windowBinder;

        public WindowManagerSubscriber( IService<IWindowManager> windowManager, IService<IWindowBinder> windowBinder )
        {
            _windowManager = windowManager;
            _windowBinder = windowBinder;
        }

        protected virtual IService<IWindowManager> WindowManager
        {
            get { return _windowManager; }
        }

        protected virtual IService<IWindowBinder> WindowBinder
        {
            get { return _windowBinder; }
        }

        public void Subscribe( string name, Window window )
        {
            _name = name;
            _window = window;

            // Window Manager
            WindowManager.ServiceStatusChanged += WindowManager_ServiceStatusChanged;
            WindowBinder.ServiceStatusChanged += WindowBinder_ServiceStatusChanged;

            if( WindowManager.Status == InternalRunningStatus.Started )
            {
                RegisterWindowManager();
            }
            if( WindowBinder.Status == InternalRunningStatus.Started )
            {
                RegisterWindowBinder();
            }
        }

        public void Unsubscribe()
        {
            UnregisterWindowManager();
            WindowManager.ServiceStatusChanged -= WindowManager_ServiceStatusChanged;
            WindowBinder.ServiceStatusChanged -= WindowBinder_ServiceStatusChanged;
        }

        public Action<WindowElementEventArgs> WindowRegistered { get; set; }

        public Action<WindowElementEventArgs> WindowUnregistered { get; set; }

        public Action<WindowBindingEventArgs> BeforeBinding { get; set; }

        public Action<WindowBindedEventArgs> AfterBinding { get; set; }

        void WindowManager_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                RegisterWindowManager();
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                UnregisterWindowManager();
            }
        }

        void WindowBinder_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                RegisterWindowBinder();
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                UnregisterWindowBinder();
            }
        }

        void RegisterWindowManager()
        {
            WindowManager.Service.Registered += Service_Registered;
            WindowManager.Service.Unregistered += Service_Unregistered;
            WindowManager.Service.RegisterWindow( _name, _window );
        }

        void UnregisterWindowManager()
        {
            if( _window != null && _window != null )
            {
                WindowManager.Service.UnregisterWindow( _name );

                WindowManager.Service.Registered -= Service_Registered;
                WindowManager.Service.Unregistered -= Service_Unregistered;
            }
        }

        private void RegisterWindowBinder()
        {
            WindowBinder.Service.BeforeBinding += Service_BeforeBinding;
            WindowBinder.Service.AfterBinding += Service_AfterBinding;
        }

        private void UnregisterWindowBinder()
        {
            WindowBinder.Service.BeforeBinding -= Service_BeforeBinding;
            WindowBinder.Service.AfterBinding -= Service_AfterBinding;
        }

        void Service_Registered( object sender, WindowElementEventArgs e )
        {
            if( WindowRegistered != null ) WindowRegistered( e );
        }

        void Service_Unregistered( object sender, WindowElementEventArgs e )
        {
            if( WindowUnregistered != null ) WindowUnregistered( e );
        }

        void Service_BeforeBinding( object sender, WindowBindingEventArgs e )
        {
            if( BeforeBinding != null ) BeforeBinding( e );
        }

        void Service_AfterBinding( object sender, WindowBindedEventArgs e )
        {
            if( AfterBinding != null ) AfterBinding( e );
        }
    }
}
