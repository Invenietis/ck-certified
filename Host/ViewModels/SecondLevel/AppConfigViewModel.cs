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
//using CK.WPF.Controls;
using CK.Plugin.Config;
using Host.Resources;
using CK.Windows.Config;
using CK.Windows;

namespace Host.VM
{
    public class AppConfigViewModel : ConfigPage
    {
        Guid _keyboardEditorId;
        AppViewModel _app;
        //SkinViewModel _sVm;
        AutoClickViewModel _acVm;
        WordPredictionViewModel _wpVm;
        ShareKeyboardViewModel _skVm;
        AppAdvancedConfigViewModel _appAdvcVm;
        ScrollingViewModel _scVm;

        public AppConfigViewModel( AppViewModel app )
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

            //JL : this feature has been removed.
            //this.AddLink( _sVm ?? ( _sVm = new SkinViewModel( _app ) ) );

            //JL : 13/12/2013 : The screenscroller editor presents performance issues when modifying parameters.
            //For now, I'll let this plugin without configuration, we'll ask the ergotherapist whether the configuration panel is necessary before spending time on it.
            //this.AddLink( _ssVm ?? ( _ssVm = new ScreenScrollerViewModel( _app ) ) );  

            //var g = this.AddGroup();

            this.AddLink( _appAdvcVm ?? (_appAdvcVm = new AppAdvancedConfigViewModel( _app )) );
            //g.AddLink( _scVm ?? (_scVm = new ScrollingViewModel( R.ScrollConfig, _app )) );
            //g.AddLink( _acVm ?? (_acVm = new AutoClickViewModel( _app )) );
            //g.AddLink( _wpVm ?? (_wpVm = new WordPredictionViewModel( _app )) );

            //g.AddLink( _skVm ?? (_skVm = new ShareKeyboardViewModel( _app )) );

            //{
            //    var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartKeyboardEditor ) );
            //    action.ImagePath = "Forward.png";
            //    action.DisplayName = R.SkinEditorSectionName;
            //    g.Items.Add( action );
            //}

            //{
            //    var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartRadarEditor ) );
            //    action.ImagePath = "Forward.png";
            //    action.DisplayName = R.RadarConfiguration;
            //    this.Items.Add( action );
            //}

            //{
            //    var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartScreenScrollerEditor ) );
            //    action.ImagePath = "Forward.png";
            //    action.DisplayName = R.ScreenScrollerConfiguration;
            //    this.Items.Add( action );
            //}

            this.AddAction( R.ObjectExplorer, R.AdvancedUserNotice, StartObjectExplorer );

            base.OnInitialize();
        }

        public void StartObjectExplorer()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{4BF2616D-ED41-4E9F-BB60-72661D71D4AF}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }

        public void StartKeyboardEditor()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( _keyboardEditorId, ConfigPluginStatus.AutomaticStart );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }

       
        public void StartRadarEditor()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{275B0E68-B880-463A-96E5-342C8E31E229}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }

        //public void StartScreenScrollerEditor()
        //{
        //    _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{652CFF65-5CF7-4FE9-8FF5-45C5E2A942E6}" ), ConfigUserAction.Started );
        //    _app.CivikeyHost.Context.PluginRunner.Apply();
        //}
    }
}
