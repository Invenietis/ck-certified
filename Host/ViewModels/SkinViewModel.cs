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
    public class SkinViewModel : ConfigPage
    {
        IObjectPluginConfig _config;
        IPluginProxy _skinPlugin;
        Guid _keyboardEditorId;
        AppViewModel _app;
        Guid _skinId;

        public SkinViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.SkinConfig;
            _app = app;
            _app.PluginRunner.PluginHost.StatusChanged += ( o, e ) => 
            {
                if( e.PluginProxy.PluginKey.PluginId == _skinId && _skinPlugin == null )
                {
                    _skinPlugin = _app.PluginRunner.PluginHost.FindLoadedPlugin( _skinId, true );
                    _config = _app.ConfigContainer.GetObjectPluginConfig( _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _skinPlugin );
                }

                NotifyOfPropertyChange( () => ActivateSkin );
                NotifyOfPropertyChange( () => EnableAutoHide );
                NotifyOfPropertyChange( () => AutoHideTimeOut );
            };
        }

        void InitializePlugin()
        {
            _skinPlugin = _app.PluginRunner.PluginHost.FindLoadedPlugin( _skinId, true );
            if( _skinPlugin != null ) _config = _app.ConfigContainer.GetObjectPluginConfig( _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _skinPlugin );
        }

        public bool ActivateSkin
        {
            get { return _skinPlugin != null ? _app.IsPluginRunning( _skinPlugin.PluginKey ) : false; }
            set
            {
                using( var waiting = _app.ShowBusyIndicator() )
                {
                    if( value )
                    {
                        _app.StartPlugin( _skinId );

                        if( _skinPlugin == null ) _skinPlugin = _app.PluginRunner.PluginHost.FindLoadedPlugin( _skinId, true );
                        _config = _skinPlugin != null ? _app.ConfigContainer.GetObjectPluginConfig( _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _skinPlugin ) : null;
                    }
                    else
                    {
                        _app.StopPlugin( _skinId );
                    }
                }

                NotifyOfPropertyChange( () => ActivateSkin );
                NotifyOfPropertyChange( () => EnableAutoHide );
                NotifyOfPropertyChange( () => AutoHideTimeOut );
            }
        }

        public bool EnableAutoHide
        {
            get { return _config != null ? _config.GetOrSet( "autohide", false ) : false; }
            set { if( _config != null ) _config.Set( "autohide", value ); }
        }

        public int AutoHideTimeOut
        {
            get { return _config != null ? _config.GetOrSet( "autohide-timeout", 6000 ) : 0; }
            set
            {
                if( _config != null ) _config.Set( "autohide-timeout", value );
            }
        }

        protected override void OnInitialize()
        {
            _skinId = new Guid( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" );
            _keyboardEditorId = new Guid( "{66AD1D1C-BF19-405D-93D3-30CA39B9E52F}" );
            InitializePlugin();

            var skinGroup = this.AddActivableSection( R.SkinSectionName.ToLower(), R.SkinConfig, this, h => h.ActivateSkin, this );

            skinGroup.AddProperty( R.SkinAutohideFeature, this, h => EnableAutoHide );

            ConfigItemMillisecondProperty p = new ConfigItemMillisecondProperty( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, ( h ) => h.AutoHideTimeOut ) );
            p.DisplayName = R.SkinAutohideTimeout;

            skinGroup.Items.Add( p );

            {
                var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartSkinEditor ) );
                action.ImagePath = "edit.png";
                action.DisplayName = R.SkinViewConfig;
                action.Description = R.AdvancedUserNotice;
                this.Items.Add( action );
            }
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

        public void StartSkinEditor()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{402C9FF7-545A-4E3C-AD35-70ED37497805}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }

        public void StartScrollEditor()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{48D3977C-EC26-48EF-8E47-806E11A1C041}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }
    }
}
