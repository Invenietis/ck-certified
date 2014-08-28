#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\ThirdLevel\ScrollingViewModel.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Reflection;
using CK.Storage;
using CK.Windows;
using CK.Windows.App;
using CK.Windows.Config;
using CommonServices;
using CommonServices.Accessibility;
using HighlightModel;
using Host.Resources;
using Scroller;

namespace Host.VM
{
    public class ScrollingViewModel : ConfigBase
    {
        ScrollingModulesConfigurationViewModel _scVm;
        AppViewModel _app;
        IObjectPluginConfig _scrollConfig;
        bool _isReady = false;

        SliderConfigItem _speedSlider;
        SliderConfigItem _turboSpeedSlider;
        ComboBoxItem _comboBox;
        RecordConfigItem _recordItem;

        Guid _scrollGuid = Guid.Parse( "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}" );

        /// <summary>
        /// Return the Scrolling user configuration, can return null if the scroller plugin isn't loaded
        /// </summary>
        //IObjectPluginConfig ScrollConfig
        //{
        //    get
        //    {
        //        if( _scrollConfig != null ) return _scrollConfig;

        //        var plugin = _app.PluginRunner.PluginHost.FindLoadedPlugin( _scrollGuid, false );
        //        if( plugin != null )
        //        {
        //            _scrollConfig = _app.ConfigContainer.GetObjectPluginConfig( _app.CivikeyHost.Context.ConfigManager.UserConfiguration, plugin );
        //            Debug.Assert( _scrollConfig != null );
        //        }
        //        return _scrollConfig;
        //    }
        //}

        public ScrollingViewModel( string displayName, AppViewModel app )
            : base( "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}", displayName, app )
        {
            _app = app;
            DisplayName = displayName;
        }

        protected override void OnInitialize()
        { 
            base.OnInitialize();

            var a = this.AddActivableSection( R.Scrolling, R.ScrollConfig);

            var g = a.AddGroup();

            _recordItem = new RecordConfigItem( _app, this, ReflectionHelper.GetPropertyInfo( this, h => h.SelectedTrigger ) );
            g.Items.Add( _recordItem );

            _comboBox = new ComboBoxItem( _app.ConfigManager, R.ScrollingStrategy, StrategyBridge.AvailableStrategies );
            if( Config != null ) _comboBox.SelectedItem = Config.GetOrSet( "Strategy", "ZoneScrollingStrategy" );
            _comboBox.SelectedItemChanged += comboBox_SelectedItemChanged;
            g.Items.Add( _comboBox );

            _speedSlider = new SliderConfigItem( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, h => h.Speed ) );
            _speedSlider.DisplayName = R.ScrollingSpeed;
            _speedSlider.SetFormatFunction( i => string.Format( "{0} s", Math.Round( i / 1000.0, 1 ) ) );
            _speedSlider.Minimum = 200;
            _speedSlider.Maximum = 5000;
            _speedSlider.Interval = 200;
            g.Items.Add( _speedSlider );

            _turboSpeedSlider = new SliderConfigItem( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, h => h.TurboSpeed ) );
            _turboSpeedSlider.DisplayName = R.TurboScrollingSpeed;
            _turboSpeedSlider.SetFormatFunction( i => string.Format( "{0} ms", i ) );
            _turboSpeedSlider.Minimum = 10;
            _turboSpeedSlider.Maximum = 500;
            _turboSpeedSlider.Interval = 10;
            g.Items.Add( _turboSpeedSlider );

            this.AddLink( _scVm ?? (_scVm = new ScrollingModulesConfigurationViewModel( R.OtherScrollConfig, _app )) );

            UpdateVisibility();

            _isReady = true;
        }

        void comboBox_SelectedItemChanged( object sender, SelectedItemChangedEventArgs e )
        {
            Strategy = e.Item;
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            _turboSpeedSlider.Visible = Strategy == "TurboScrollingStrategy";
        }

        public int Speed
        {
            get { return (Config != null) ? Config.GetOrSet( "Speed", 1000 ) : 0; }
            set
            {
                if( Config != null ) Config["Speed"] = value;
            }
        }

        public int TurboSpeed
        {
            get { return (Config != null) ? Config.GetOrSet( "TurboSpeed", 100 ) : 0; }
            set
            {
                if( Config != null ) Config["TurboSpeed"] = value;
            }
        }

        public string Strategy
        {
            get { return (Config != null) ? Config.GetOrSet( "Strategy", "ZoneScrollingStrategy" ) : string.Empty; }
            set
            {
                if( Config != null ) Config["Strategy"] = value;
            }
        }

        public ITrigger SelectedTrigger
        {
            get { return (Config != null) ? Config.GetOrSet( "Trigger", (ITrigger)null ) : (ITrigger)null; }
            set
            {
                if( Config != null ) Config["Trigger"] = value;
            }
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            NotifyOfPropertyChange( () => Speed );
            NotifyOfPropertyChange( () => TurboSpeed );
            NotifyOfPropertyChange( () => Strategy );
            NotifyOfPropertyChange( () => SelectedTrigger );
        }


        protected override void NotifyOfPropertiesChange()
        {
            if( _isReady && ActivatePlugin )
            {
                if( Config != null ) _comboBox.SelectedItem = Config.GetOrSet( "Strategy", "TurboScrollingStrategy" );
                _recordItem.Refresh();
                _comboBox.Refresh();
                _speedSlider.Refresh();
                _turboSpeedSlider.Refresh();
            }
            NotifyOfPropertyChange( () => ActivatePlugin );
            NotifyOfPropertyChange( () => Speed );
            NotifyOfPropertyChange( () => TurboSpeed );
            NotifyOfPropertyChange( () => Strategy );
            NotifyOfPropertyChange( () => SelectedTrigger );
        }
    }
}
