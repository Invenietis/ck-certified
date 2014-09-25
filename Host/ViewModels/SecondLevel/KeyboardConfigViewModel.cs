#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\AppConfigViewModel.cs) is part of CiviKey. 
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
using CK.Plugin.Config;
using CK.Windows;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    public class KeyboardConfigViewModel : ConfigPage
    {
        Guid _keyboardEditorId;
        AppViewModel _app;
        ShareKeyboardViewModel _skVm;

        public KeyboardConfigViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.AppConfig;
            _app = app;
        } 

        protected override void OnInitialize()
        {
            _keyboardEditorId = new Guid( "{66AD1D1C-BF19-405D-93D3-30CA39B9E52F}" );

            var profiles = this.AddCurrentItem( R.Profile, "", _app.CivikeyHost.Context.ConfigManager.SystemConfiguration, a => a.CurrentUserProfile, a => a.UserProfiles, false, "" );
            _app.CivikeyHost.Context.ConfigManager.SystemConfiguration.UserProfiles.CollectionChanged += ( s, e ) =>
            {
                profiles.RefreshValues( s, e );
            };

            this.AddLink( _skVm ?? (_skVm = new ShareKeyboardViewModel( _app )) );

            {
                var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartKeyboardEditor ) );
                action.ImagePath = "Forward.png";
                action.DisplayName = R.SkinEditorSectionName;
                this.Items.Add( action );
            }

            base.OnInitialize();
        }

        public void StartKeyboardEditor()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( _keyboardEditorId, ConfigPluginStatus.AutomaticStart );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }
    }
}
