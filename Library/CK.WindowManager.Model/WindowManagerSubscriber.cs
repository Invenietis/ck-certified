#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WindowManager.Model\WindowManagerSubscriber.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
            if( WindowManager.Status.IsStartingOrStarted ) UnregisterWindowManager();
            WindowManager.ServiceStatusChanged -= WindowManager_ServiceStatusChanged;
            
            if( WindowBinder.Status.IsStartingOrStarted ) UnregisterWindowBinder();
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
            else if( e.Current == InternalRunningStatus.Stopped )
            {
                if( OnBinderStopped != null ) OnBinderStopped();
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
            if( OnBinderStarted != null && WindowBinder.Status == InternalRunningStatus.Started ) OnBinderStarted();
        }

        protected void UnregisterWindowBinder()
        {
            WindowBinder.Service.BeforeBinding -= Service_BeforeBinding;
            WindowBinder.Service.AfterBinding -= Service_AfterBinding;
            if( OnBinderStopped != null && WindowBinder.Status == InternalRunningStatus.Stopped ) OnBinderStopped();
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
