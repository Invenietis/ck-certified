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

namespace Host.VM
{
    public class AutoClickViewModel : ConfigBase
    {
        public AutoClickViewModel( AppViewModel app )
            : base( "{989BE0E6-D710-489e-918F-FBB8700E2BB2}", R.AutoClickConfig, app )
        {
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
            base.OnInitialize();

            var g = this.AddActivableSection( R.AutoClickSectionName.ToLower(), R.AutoClickConfig );

            ConfigItemMillisecondProperty p2 = new ConfigItemMillisecondProperty( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, h => h.TimeBeforeCountDownStarts ) );
            p2.DisplayName = R.AutoClickTimeBeforeCountDownStarts;
            g.Items.Add( p2 );

            ConfigItemMillisecondProperty p = new ConfigItemMillisecondProperty( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, h => h.CountDownDuration ) );
            p.DisplayName = R.AutoClickCountDownDuration;
            g.Items.Add( p );

            g.AddProperty( R.AutoClickShowMousePanelOption, this, h => ShowMouseIndicatorOption );
        }
    }
}
