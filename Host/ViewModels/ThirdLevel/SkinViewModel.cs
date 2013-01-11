#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\SkinViewModel.cs) is part of CiviKey. 
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

using CK.Core;
//using CK.WPF.Controls;
using Host.Resources;
using System;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Reflection;
using System.Windows.Input;
using CK.Windows.Config;
using CK.Windows;

namespace Host.VM
{
    public class SkinConfigViewModel : ConfigPage
    {
        IObjectPluginConfig _config;
        IPluginProxy _skinPlugin;
        Guid _keyboardEditorId;
        AppViewModel _app;
        Guid _skinId;
        SkinBasicConfigViewModel _skinBasicConfVm;

        public SkinConfigViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.SkinConfig;
            _app = app;
        }

       
        protected override void OnInitialize()
        {
            _skinId = new Guid( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" );
            _keyboardEditorId = new Guid( "{66AD1D1C-BF19-405D-93D3-30CA39B9E52F}" );

            this.AddLink( _skinBasicConfVm ?? new SkinBasicConfigViewModel( _app ));

            {
                var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartScrollEditor ) );
                action.ImagePath = "edit.png";
                action.DisplayName = R.ScrollConfig;
                action.Description = R.AdvancedUserNotice;
                this.Items.Add( action );
            }
            var editionGroup = this.AddGroup();
            var keyboardEditorStarter = new ConfigFeatureStarter( ConfigManager, _app.PluginRunner, _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _keyboardEditorId ) { DisplayName = R.SkinEditorSectionName };
            editionGroup.Items.Add( keyboardEditorStarter );

            base.OnInitialize();
        }

        public void StartScrollEditor()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{48D3977C-EC26-48EF-8E47-806E11A1C041}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }
    }
}
