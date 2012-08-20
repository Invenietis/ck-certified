﻿using System.Windows.Forms.Integration;
using CK.Context;
using CK.Plugin;
using CK.Plugin.Config;
using Caliburn.Micro;
using System.Windows;
using CommonServices;
using Host.Services;

namespace CK.Plugins.ObjectExplorer
{
    [Plugin( StrPluginID, PublicName = "Object Explorer", Version = "1.0.0", Categories = new string[] { "Advanced" },
     IconUri = "Plugins/ObjectExplorer/UI/Resources/objectExplorerIcon.ico" )]
    public class ObjectExplorer : IPlugin
    {
        public const string StrPluginID = "{4BF2616D-ED41-4E9F-BB60-72661D71D4AF}";
        WindowManager _wnd;
        Window _mainWindow;

        public VMIContextViewModel VMIContext { get; private set; }

        [RequiredService( Required = true )]
        public INotificationService Notification { get; set; }

        [RequiredService( Required = true )]
        public IContext Context { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public ILogService LogService { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            VMIContext = new VMIContextViewModel( Context, Config, LogService );

            VMIContextView view = new VMIContextView();
            view.DataContext = VMIContext;
            view.Show();

            //Using Caliburn.micro enslaves this window, so when the host is hidden, the object explorer is hidden as well.
            //_wnd = new WindowManager();
            //_wnd.ShowWindow( VMIContext );
            
            //_mainWindow = VMIContext.GetView( null ) as Window;
            //ElementHost.EnableModelessKeyboardInterop( _mainWindow );
        }

        public void Stop()
        {
            if( !VMIContext.Closing )
            {
                VMIContext.ManualStop = true;
                VMIContext.TryClose();
            }
        }

        public void Teardown()
        {
        }
    }
}
