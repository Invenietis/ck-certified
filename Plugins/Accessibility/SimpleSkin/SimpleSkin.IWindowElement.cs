﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.WindowManager.Model;

namespace SimpleSkin
{
    public partial class SkinWindow : IWindowElement
    {
        public IWindowElement2 WindowElement { get; set; }
    }

    public partial class SimpleSkin
    {
        WindowElement _w;

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        partial void OnSuccessfulStart()
        {
            WindowManager.ServiceStatusChanged += WindowManager_ServiceStatusChanged;
            if( WindowManager.Status == InternalRunningStatus.Started )
            {
                RegisterWindowManager();
            }
        }

        partial void OnSuccessfulStop()
        {
            UnregisterWindowManager();
            WindowManager.ServiceStatusChanged -= WindowManager_ServiceStatusChanged;
        }

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


        private void RegisterWindowManager()
        {
            _skinDispatcher.BeginInvoke( new Action( () =>
            {
                _skinWindow.WindowElement = new WindowElement( _skinWindow, "Skin" );
                WindowManager.Service.Register( _skinWindow );
            } ) );
        }

        private void UnregisterWindowManager()
        {

            _skinDispatcher.BeginInvoke( new Action( () =>
            {
                WindowManager.Service.Unregister( _skinWindow );
                _skinWindow.WindowElement.Dispose();
            } ) );
        }
    }
}
