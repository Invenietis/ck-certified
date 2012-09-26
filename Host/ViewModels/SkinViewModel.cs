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

namespace Host.VM
{
    public class SkinViewModel : ConfigPage
    {
        AppViewModel _app;
        IPluginProxy _skinPlugin;
        Guid _skinId;
        IObjectPluginConfig _config;

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
            InitializePlugin();

            var g = this.AddActivableSection( R.SkinSectionName.ToLower(), R.SkinConfig, this, h => h.ActivateSkin, this );

            g.AddProperty( R.SkinAutohideFeature, this, h => EnableAutoHide );

            ConfigItemMillisecondProperty p = new ConfigItemMillisecondProperty( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, ( h ) => h.AutoHideTimeOut ) );
            p.DisplayName = R.SkinAutohideTimeout;
            g.Items.Add( p );

            base.OnInitialize();
        }
    }
}
