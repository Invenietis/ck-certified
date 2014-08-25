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
* Copyright © 2007-2014, 
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
using CK.Storage;
using CK.Windows;
using CK.Windows.App;
using CK.Windows.Config;
using CommonServices.Accessibility;
using HighlightModel;
using Host.Resources;

namespace Host.VM
{
    public class ScrollingViewModel : ConfigPage
    {
        ScrollingModulesConfigurationViewModel _scVm;
        AppViewModel _app;

        public ScrollingViewModel( string displayName, AppViewModel app )
            : base( app.ConfigManager )
        {
            _app = app;
            DisplayName = displayName;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartScrollEditor ) );
            action.ImagePath = "Forward.png";
            action.DisplayName = R.ScrollConfig;
            this.Items.Add( action );

            this.AddLink( _scVm ?? (_scVm = new ScrollingModulesConfigurationViewModel( R.OtherScrollConfig, _app )) );

            
        }

        public void StartScrollEditor()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{48D3977C-EC26-48EF-8E47-806E11A1C041}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }


    }
}
