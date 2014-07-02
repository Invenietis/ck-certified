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
using CK.Storage;
using CK.Windows;
using CK.Windows.App;
using CK.Windows.Config;
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

        SliderConfigItem _speedSlider;
        SliderConfigItem _turboSpeedSlider;

        Guid _scrollGuid = Guid.Parse( "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}" );


        public ScrollingViewModel( string displayName, AppViewModel app )
            : base( "{4E3A3B25-7FD0-406F-A958-ECB50AC6A597}", displayName, app )
        {
            _app = app;
            DisplayName = displayName;

            var plugin = _app.PluginRunner.PluginHost.FindLoadedPlugin( _scrollGuid, true );
            if( plugin != null )
            {
                _scrollConfig = _app.ConfigContainer.GetObjectPluginConfig( _app.CivikeyHost.Context.ConfigManager.UserConfiguration, plugin );
                Debug.Assert( _scrollConfig != null );
            }
        }

        protected override void OnInitialize()
        { 
            base.OnInitialize();

            //var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartScrollEditor ) );
            //action.ImagePath = "Forward.png";
            //action.DisplayName = R.ScrollConfig;

            var g = this.AddGroup();

            var comboBox = new ComboBoxItem( _app.ConfigManager, "Scroll type :", StrategyBridge.AvailableStrategies );
            comboBox.SelectedItem = _scrollConfig.GetOrSet( "Strategy", "TurboScrollingStrategy" );
            comboBox.SelectedItemChanged += comboBox_SelectedItemChanged;
            g.Items.Add( comboBox );

            _speedSlider = new SliderConfigItem( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, h => h.Speed ) );
            _speedSlider.DisplayName = "blabla";
            _speedSlider.SetFormatFunction( i => string.Format( "{0} s", Math.Round( i / 1000.0, 1 ) ) );
            _speedSlider.Minimum = 200;
            _speedSlider.Maximum = 5000;
            _speedSlider.Interval = 200;
            g.Items.Add( _speedSlider );

            _turboSpeedSlider = new SliderConfigItem( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, h => h.TurboSpeed ) );
            _turboSpeedSlider.DisplayName = "blabla";
            _turboSpeedSlider.SetFormatFunction( i => string.Format( "{0} ms", i ) );
            _turboSpeedSlider.Minimum = 10;
            _turboSpeedSlider.Maximum = 500;
            _turboSpeedSlider.Interval = 10;
            g.Items.Add( _turboSpeedSlider );

            //g.Items.Add( action );
            this.AddLink( _scVm ?? (_scVm = new ScrollingModulesConfigurationViewModel( R.OtherScrollConfig, _app )) );

            UpdateVisibility();
        }

        void comboBox_SelectedItemChanged( object sender, SelectedItemChangedEventArgs e )
        {
            Strategy = e.Item;
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if( Strategy == "TurboScrollingStrategy" )
            {
                _speedSlider.Visible = false;
                _turboSpeedSlider.Visible = true;
            }
            else
            {
                _turboSpeedSlider.Visible = false;
                _speedSlider.Visible = true;
            }
        }

        public int Speed
        {
            get { return _scrollConfig.GetOrSet( "Speed", 1000 ); }
            set
            {
                if( _scrollConfig != null ) _scrollConfig["Speed"] = value;
            }
        }

        public int TurboSpeed
        {
            get { return _scrollConfig.GetOrSet( "TurboSpeed", 100 ); }
            set
            {
                _scrollConfig["TurboSpeed"] = value;
            }
        }

        public string Strategy
        {
            get { return _scrollConfig.GetOrSet( "Strategy", "ZoneScrollingStrategy" ); }
            set
            {
                _scrollConfig["Strategy"] = value;
            }
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            NotifyOfPropertyChange( () => Speed );
            NotifyOfPropertyChange( () => TurboSpeed );
            NotifyOfPropertyChange( () => Strategy );
        }
    }
}
