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
using System;
using CommonServices.Accessibility;
using CK.Core;

namespace CK.Plugins.ObjectExplorer
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion, Categories = new string[] { "Advanced" },
     IconUri = "Plugins/ObjectExplorer/UI/Resources/objectExplorerIcon.ico" )]
    public class ObjectExplorer : IPlugin
    {
        const string PluginGuidString = "{4BF2616D-ED41-4E9F-BB60-72661D71D4AF}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Object Explorer";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginIdVersion, PluginPublicName );
        
        //WindowManager _wnd;
        //Window _mainWindow;
        VMIContextView _view;

        public VMIContextViewModel VMIContext { get; private set; }

        [RequiredService( Required = true )]
        public INotificationService Notification { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IHelpService HelpService { get; set; }

        [RequiredService( Required = true )]
        public IContext Context { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public ILogService LogService { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        bool _isClosing;

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            VMIContext = new VMIContextViewModel( Context, Config, LogService, HelpService );

            _view = new VMIContextView();
            _view.DataContext = VMIContext;
            _view.Closing += OnViewClosing;
            _view.Show();
        }

        void OnViewClosing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            if( !_isClosing )
            {
                _isClosing = true;
                Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( PluginGuidString ), ConfigUserAction.Stopped );
                Context.PluginRunner.Apply();
                return;
            }
            else
            {
                _isClosing = false;
                VMIContext = null;
            }
        }

        public void Stop()
        {
            if( !_isClosing )
            {
                _isClosing = true;
                _view.Close();
            }
            else
            {
                _isClosing = false;
                VMIContext = null;
            }
        }

        public void Teardown()
        {
        }
    }
}
