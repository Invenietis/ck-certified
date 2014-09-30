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

using System.Windows.Media;
using CK.Plugin.Config;
using CK.Reflection;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    public class WindowConfigViewModel : ConfigBase
    {
        ScrollingModulesConfigurationViewModel _scVm;
        AppViewModel _app;
        bool _isReady = false;

        SliderConfigItem _windowOpacitySlider;
        ConfigItemProperty<Color> _windowBackgroundColor;
        ConfigItemProperty<Color> _windowBorderBrush;

        public WindowConfigViewModel( string displayName, AppViewModel app )
            : base( "{BCD4DE84-E6C9-47C3-B29D-3EAA0D50B14C}", displayName, app ) //SharedDataPlugin
        {
            _app = app;
            DisplayName = displayName;

            //this setting page always need SharedDataPlugin
            this.ActivatePlugin = true;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            var g = this.AddGroup();

            _windowBackgroundColor = new ConfigItemProperty<Color>( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, e => e.WindowBackgroundColor ) );
            _windowBackgroundColor.DisplayName = R.WindowBackgroundColor;
            g.Items.Add( _windowBackgroundColor );

            _windowBorderBrush = new ConfigItemProperty<Color>( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, e => e.WindowBorderBrush ) );
            _windowBorderBrush.DisplayName = R.WindowBorderBrush;
            g.Items.Add( _windowBorderBrush );

            _windowOpacitySlider = new SliderConfigItem( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, h => h.WindowOpacity ) );
            _windowOpacitySlider.DisplayName = R.WindowOpacity;
            _windowOpacitySlider.SetFormatFunction( i => string.Format( "{0} %", i ) );
            _windowOpacitySlider.Minimum = 0;
            _windowOpacitySlider.Maximum = 100;
            _windowOpacitySlider.Interval = 5;
            g.Items.Add( _windowOpacitySlider );

            _isReady = true;
        }

        public int WindowOpacity
        {
            get { return (Config != null) ? (int)(Config.GetOrSet<double>( "WindowOpacity", 1 ) * 100) : 100; }
            set
            {
                if( Config != null )
                {
                    if( value == 0 ) Config["WindowOpacity"] = 0.01;
                    else Config["WindowOpacity"] = ((double)value) / 100;
                }
            }
        }

        public Color WindowBackgroundColor
        {
            get 
            { 
                return Config != null ? Config.GetOrSet<Color>( "WindowBackgroundColor", (Color)ColorConverter.ConvertFromString( "#4F4F4F" ) ) : (Color)ColorConverter.ConvertFromString( "#4F4F4F" ); 
            }
            set
            {
                if( Config != null )
                {
                    Config["WindowBackgroundColor"] = value;
                }
            }
        }

        public Color WindowBorderBrush
        {
            get
            {
                return Config != null ? Config.GetOrSet<Color>( "WindowBorderBrush", (Color)ColorConverter.ConvertFromString( "#4F4F4F" ) ) : (Color)ColorConverter.ConvertFromString( "#4F4F4F" );
            }
            set
            {
                if( Config != null )
                {
                    Config["WindowBorderBrush"] = value;
                }
            }
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e != null )
            {
                if( e.Key == "WindowOpacity" ) NotifyOfPropertyChange( "WindowOpacity" );
                else if( e.Key == "WindowBackgroundColor" ) NotifyOfPropertyChange( "WindowBackgroundColor" );
                else if( e.Key == "WindowBorderBrush" ) NotifyOfPropertyChange( "WindowBorderBrush" );
            }
        }
    }
}
