#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\ThirdLevel\ShareKeyboardViewModel.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.IO;
using CK.Plugin.Config;
using CK.Windows;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    public class ShareKeyboardViewModel : ConfigPage
    {
        AppViewModel _app;

        public ShareKeyboardViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.ShareKeyboard;
            _app = app;
        }

        protected override void OnInitialize()
        {
            var profiles = this.AddCurrentItem( R.Profile, "", _app.CivikeyHost.Context.ConfigManager.SystemConfiguration, a => a.CurrentUserProfile, a => a.UserProfiles, false, "" );
            _app.CivikeyHost.Context.ConfigManager.SystemConfiguration.UserProfiles.CollectionChanged += ( s, e ) =>
            {
                profiles.RefreshValues( s, e );
            };

            {
                var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartImport ) );
                string a = Directory.GetCurrentDirectory();
                action.ImagePath = "pack://application:,,,/CiviKey;component/Resources/Images/Windows.png";
                action.DisplayName = R.ImportKeyboard;
                this.Items.Add( action );
            }

            {
                var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartExport) );
                action.ImagePath = "pack://application:,,,/CiviKey;component/Resources/Images/Windows.png";
                action.DisplayName = R.ExportKeyboard;
                this.Items.Add( action );
            }

            base.OnInitialize();
        }

        public void StartImport()
        {

            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{D94D1757-5BFB-4B80-9C8E-1B108F5C7086}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }

        public void StartExport()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{244C578B-322A-4733-A34B-EEC0558F61D5}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }
    }
}
