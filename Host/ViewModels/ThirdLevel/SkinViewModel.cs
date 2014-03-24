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

//using CK.WPF.Controls;
using Host.Resources;
using CK.Plugin.Config;
using CK.Reflection;
using CK.Windows.Config;
using CK.Windows;
using System;

namespace Host.VM
{
    public class SkinViewModel : ConfigBase
    {
        AppViewModel _app;
        Action _action;

        public SkinViewModel( AppViewModel app )
            : base( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}", R.SkinConfig, app )
        {
            _app = app;

            _action = () =>
            {
                OnPropertyChanged( "EnableAutoHide" );
                OnPropertyChanged( "AutoHideTimeOut" );
            };
        }

        protected override void NotifyOfPropertiesChange()
        {
            base.NotifyOfPropertiesChange();
            DispatchPropertyChanged();
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            DispatchPropertyChanged();
        }

        void DispatchPropertyChanged()
        {
            if( NoFocusManager.Default.NoFocusDispatcher.CheckAccess() )
                _action();
            else
                NoFocusManager.Default.NoFocusDispatcher.BeginInvoke( _action );
        }

        public bool EnableAutoHide
        {
            get { return Config != null ? Config.GetOrSet( "autohide", false ) : false; }
            set { if( Config != null ) Config.Set( "autohide", value ); }
        }

        public int AutoHideTimeOut
        {
            get { return Config != null ? Config.GetOrSet( "autohide-timeout", 6000 ) : 0; }
            set
            {
                if( Config != null ) Config.Set( "autohide-timeout", value );

            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            var skinGroup = this.AddActivableSection( R.SkinSectionName.ToLower(), R.SkinConfig );

            skinGroup.AddProperty( R.SkinAutohideFeature, this, h => EnableAutoHide );

            ConfigItemMillisecondProperty p = new ConfigItemMillisecondProperty( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, ( h ) => h.AutoHideTimeOut ) );
            p.DisplayName = R.SkinAutohideTimeout;

            skinGroup.Items.Add( p );

            base.OnInitialize();
        }


    }
}
