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
    public class WindowConfigViewModel : ConfigBase
    {
        ScrollingModulesConfigurationViewModel _scVm;
        AppViewModel _app;
        bool _isReady = false;

        SliderConfigItem _windowOpacitySlider;

        public WindowConfigViewModel( string displayName, AppViewModel app )
            : base( "{BCD4DE84-E6C9-47C3-B29D-3EAA0D50B14C}", displayName, app )
        {
            _app = app;
            DisplayName = displayName;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            var a = this.AddActivableSection( R.Scrolling, R.ScrollConfig );

            var g = a.AddGroup();

            _windowOpacitySlider = new SliderConfigItem( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, h => h.WindowOpacity ) );
            _windowOpacitySlider.DisplayName = R.TurboScrollingSpeed;
            _windowOpacitySlider.SetFormatFunction( i => string.Format( "{0} %", i ) );
            _windowOpacitySlider.Minimum = 1;
            _windowOpacitySlider.Maximum = 100;
            _windowOpacitySlider.Interval = 5;
            g.Items.Add( _windowOpacitySlider );

            this.AddLink( _scVm ?? (_scVm = new ScrollingModulesConfigurationViewModel( R.OtherScrollConfig, _app )) );

            _isReady = true;
        }

        public int WindowOpacity
        {
            get { return (Config != null) ? (int)(Config.GetOrSet<double>( "WindowOpacity", 1 ) * 100) : 100; }
            set
            {
                if( Config != null ) Config["WindowOpacity"] = ((double)value)/100;
            }
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            NotifyOfPropertyChange( () => WindowOpacity );
        }


        protected override void NotifyOfPropertiesChange()
        {
            if( _isReady && ActivatePlugin )
            {
                _windowOpacitySlider.Refresh();
            }
            NotifyOfPropertyChange( () => ActivatePlugin );
            NotifyOfPropertyChange( () => WindowOpacity );
        }
    }
}
