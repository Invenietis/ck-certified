#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\AutoClickViewModel.cs) is part of CiviKey. 
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

//using CK.WPF.Controls;
using CK.Plugin.Config;
using Host.Resources;
using CK.Windows.Config;
using CK.Plugin;
using System;
using System.Linq;
using System.Collections.Generic;
using CK.Windows.App;
using CK.Utils;

namespace Host.VM
{
    public class AutoClickViewModel : ConfigBase
    {
        readonly ISimplePluginRunner _runner;
        readonly IUserConfiguration _userConf;

        readonly Guid _clickSelectorHoverId = new Guid( "{F9687F04-7370-4812-9EB4-1320EB282DD8}" );
        readonly Guid  _clickSelectorScrollerId = new Guid( "{1986E566-7426-44DC-ACA3-9E8E8EB673B8}" );
        readonly List<ConfigImplementationSelectorItem> _items;

        Guid _previous;
        Guid _current;

        public AutoClickViewModel( AppViewModel app )
            : base( "{989BE0E6-D710-489e-918F-FBB8700E2BB2}", R.AutoClickConfig, app )
        {
            _runner = app.PluginRunner;
            _userConf = _app.CivikeyHost.Context.ConfigManager.UserConfiguration;
            _items = new List<ConfigImplementationSelectorItem>();
        }

        protected override void NotifyOfPropertiesChange()
        {
            NotifyOfPropertyChange( () => ActivatePlugin );
            NotifyOfPropertyChange( () => CountDownDuration );
            NotifyOfPropertyChange( () => TimeBeforeCountDownStarts );
            NotifyOfPropertyChange( () => ShowMouseIndicatorOption );
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            NotifyOfPropertyChange( () => CountDownDuration );
            NotifyOfPropertyChange( () => TimeBeforeCountDownStarts );
            NotifyOfPropertyChange( () => ShowMouseIndicatorOption );
        }

        public int CountDownDuration
        {
            get { return Config != null ? Config.GetOrSet( "CountDownDuration", 2000 ) : 0; }
            set
            {
                if( Config != null ) Config.Set( "CountDownDuration", value );
            }
        }

        Guid SelectorPlugin
        {
            get { return _app.CivikeyHost.UserConfig.GetOrSet( "ClickSelector_Plugin", _clickSelectorHoverId ); }
            set { _app.CivikeyHost.UserConfig.Set( "ClickSelector_Plugin", value ); }
        }

        public int TimeBeforeCountDownStarts
        {
            get { return Config != null ? Config.GetOrSet( "TimeBeforeCountDownStarts", 1500 ) : 0; }
            set
            {
                if( Config != null ) Config.Set( "TimeBeforeCountDownStarts", value );
            }
        }

        public bool ShowMouseIndicatorOption
        {
            get { return Config != null ? Config.GetOrSet( "ShowMouseIndicatorOption", false ) : false; }
            set
            {
                if( Config != null ) Config.Set( "ShowMouseIndicatorOption", value );
            }
        }

        protected override void OnInitialize()
        {
            Guid defaultPlugin = GetDefaultItem( _clickSelectorHoverId, _clickSelectorScrollerId );
            _previous = defaultPlugin;
            _current = defaultPlugin;

            var g = this.AddActivableSection( R.AutoClickSectionName.ToLower(), R.AutoClickConfig );

            ConfigItemMillisecondProperty p2 = new ConfigItemMillisecondProperty( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, h => h.TimeBeforeCountDownStarts ) );
            p2.DisplayName = R.AutoClickTimeBeforeCountDownStarts;
            g.Items.Add( p2 );

            ConfigItemMillisecondProperty p = new ConfigItemMillisecondProperty( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, h => h.CountDownDuration ) );
            p.DisplayName = R.AutoClickCountDownDuration;
            g.Items.Add( p );

            g.AddProperty( R.AutoClickShowMousePanelOption, this, h => ShowMouseIndicatorOption );

            var hover = new ConfigImplementationSelectorItem( _app.ConfigManager, new PluginCluster( _runner, _userConf, () => _clickSelectorHoverId ), "clickselector" );
            hover.DisplayName = R.ClickTypeSelectorName;
            hover.Description = R.ClickTypeSelectorName;
            hover.IsDefaultItem = defaultPlugin == _clickSelectorHoverId || defaultPlugin == Guid.Empty;
            Items.Add( hover );
            _items.Add( hover );

            var scroll = new ConfigImplementationSelectorItem( _app.ConfigManager, new PluginCluster( _runner, _userConf, () => _clickSelectorScrollerId ), "clickselector" );
            scroll.DisplayName = R.ClickTypeScrollerSelector;
            scroll.Description = R.ClickTypeScrollerSelector;
            scroll.IsDefaultItem = defaultPlugin == _clickSelectorScrollerId;
            Items.Add( scroll );
            _items.Add( scroll );

            var apply = new RadioConfigItemApply( _app.ConfigManager, new VMCommand( Apply ), hover, scroll );
            apply.DisplayName = R.Apply;
            Items.Add( apply );

            base.OnInitialize();
        }

        private void SetConfigStopped()
        {
            foreach( var i in _items )
            {
                if( !i.IsSelected )
                {
                    i.PluginCluster.StopPlugin();
                }
            }
        }

        private void SetConfigStarted()
        {
            foreach( var i in _items )
            {
                if( i.IsSelected )
                {
                    i.PluginCluster.StartPlugin();
                }
            }
        }

        private void Apply()
        {
            _previous = _current;

            SetConfigStopped();
            SetConfigStarted();
            _runner.Apply();

            _items.Single( ( i ) => i.IsSelected );
        }

        private Guid GetDefaultItem( params Guid[] ids )
        {
            if( ids.Count( i => _runner.PluginHost.IsPluginRunning( i ) ) == 1 ) return ids.First( i => _runner.PluginHost.IsPluginRunning( i ) );
            return Guid.Empty;
        }
    }
}
