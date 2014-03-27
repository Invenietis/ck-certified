using System;
using CK.Plugin;
using CK.Windows;

namespace CK.WindowManager.Model
{
    public class WindowManagerSubscriber
    {
        string _name;
        CKWindow _window;
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

        public virtual void Subscribe( string name, CKWindow window )
        {
            _name = name;
            _window = window;

            WindowManager.ServiceStatusChanged += WindowManager_ServiceStatusChanged;
            if( WindowManager.Status == InternalRunningStatus.Started )
            {
                RegisterWindowManager();
            }

            WindowBinder.ServiceStatusChanged += WindowBinder_ServiceStatusChanged;
            if( WindowBinder.Status == InternalRunningStatus.Started )
            {
                RegisterWindowBinder();
            }
        }

        public virtual void Unsubscribe()
        {
            UnregisterWindowManager();
            WindowManager.ServiceStatusChanged -= WindowManager_ServiceStatusChanged;
            
            UnregisterWindowBinder();
            WindowBinder.ServiceStatusChanged -= WindowBinder_ServiceStatusChanged;
        }

        public Action<WindowElementEventArgs> WindowRegistered { get; set; }

        public Action<WindowElementEventArgs> WindowUnregistered { get; set; }

        public Action<WindowBindingEventArgs> BeforeBinding { get; set; }

        public Action<WindowBindedEventArgs> AfterBinding { get; set; }

        public Action OnBinderStarted { get; set; }

        public Action OnBinderStopped { get; set; }

        protected void WindowManager_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
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

        protected void WindowBinder_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
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

        protected void RegisterWindowManager()
        {
            WindowManager.Service.Registered += Service_Registered;
            WindowManager.Service.Unregistered += Service_Unregistered;
            WindowManager.Service.RegisterWindow( _name, _window );
        }

        protected void UnregisterWindowManager()
        {
            if( _window != null )
            {
                WindowManager.Service.UnregisterWindow( _name );

                WindowManager.Service.Registered -= Service_Registered;
                WindowManager.Service.Unregistered -= Service_Unregistered;
            }
        }

        protected void RegisterWindowBinder()
        {
            WindowBinder.Service.BeforeBinding += Service_BeforeBinding;
            WindowBinder.Service.AfterBinding += Service_AfterBinding;
            if( OnBinderStarted != null ) OnBinderStarted();
        }

        protected void UnregisterWindowBinder()
        {
            if( OnBinderStopped != null ) OnBinderStopped();
            WindowBinder.Service.BeforeBinding -= Service_BeforeBinding;
            WindowBinder.Service.AfterBinding -= Service_AfterBinding;
        }

        protected void Service_Registered( object sender, WindowElementEventArgs e )
        {
            if( WindowRegistered != null ) WindowRegistered( e );
        }

        protected void Service_Unregistered( object sender, WindowElementEventArgs e )
        {
            if( WindowUnregistered != null ) WindowUnregistered( e );
        }

        protected void Service_BeforeBinding( object sender, WindowBindingEventArgs e )
        {
            if( BeforeBinding != null ) BeforeBinding( e );
        }

        protected void Service_AfterBinding( object sender, WindowBindedEventArgs e )
        {
            if( AfterBinding != null ) AfterBinding( e );
        }
    }
}
