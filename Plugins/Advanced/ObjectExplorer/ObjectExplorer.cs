#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ObjectExplorer.cs) is part of CiviKey. 
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

using System.Windows.Forms.Integration;
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
        //WindowManager _wnd;
        //Window _mainWindow;
        VMIContextView _view;

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

            _view = new VMIContextView();
            _view.DataContext = VMIContext;
            _view.Show();

            //Using Caliburn.micro enslaves this window, so when the host is hidden, the object explorer is hidden as well.
            //_wnd = new WindowManager();
            //_wnd.ShowWindow( VMIContext );
            
            //_mainWindow = VMIContext.GetView( null ) as Window;
            //ElementHost.EnableModelessKeyboardInterop( _mainWindow );
        }

        public void Stop()
        {
            _view.Close();
            //if( !VMIContext.Closing )
            //{
            //    VMIContext.ManualStop = true;
            //    VMIContext.TryClose();
            //}
        }

        public void Teardown()
        {
        }
    }
}
