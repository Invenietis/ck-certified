#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\RootConfigViewModel.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using CK.Plugin.Config;
using CK.Core;
using CK.Reflection;
using Host.Resources;
using CK.Keyboard.Model;
using CK.Windows.Config;
using Host.VM;

namespace Host
{
    //First level of the civikey host
    public class RootConfigViewModel : CK.Windows.Config.ConfigPage
    {
        AppViewModel _app;
        AppConfigViewModel _appConfigVm;
        Guid _autoclicId;
        Guid _skinId;
        ConfigItemCurrent<IKeyboard> _keyboards;

        public RootConfigViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = Resources.R.Home;
            _app = app;
            _autoclicId = new Guid( "{989BE0E6-D710-489e-918F-FBB8700E2BB2}" );
            _skinId = new Guid( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" );
        }

        private void RefreshKeyboardValues( object o, EventArgs e )
        {
            //JL : that fix stinks like hell.
            //When calling RefreshValues, the current selected item is set to null. (actually setting the current keyboard to null)
            //Can't figure out why yet.
            IKeyboard k = _app.KeyboardContext.CurrentKeyboard;
            _keyboards.RefreshValues( o, e );
            _keyboards.Values.MoveCurrentTo( k );
        }

        protected override void OnInitialize()
        {
            if( _app.KeyboardContext != null )
            {
                _keyboards = this.AddCurrentItem( R.Keyboard, null, _app.KeyboardContext, c => c.CurrentKeyboard, c => c.Keyboards, false, "" );
                _keyboards.ImagePath = "/Views/Images/Keyboard.png";//"pack://application:,,,/CK-Certified;component/Views/Images/Keyboard.png"

                _app.KeyboardContext.Keyboards.KeyboardCreated += ( s, e ) => 
                {
                    RefreshKeyboardValues(s,e); 
                };

                _app.KeyboardContext.Keyboards.KeyboardDestroyed += ( s, e ) => 
                {
                    RefreshKeyboardValues(s,e);
                };

                _app.KeyboardContext.Keyboards.KeyboardRenamed += ( s, e ) => 
                {
                    //When renaming a keyboard, the value is removed and then added back.
                    //The ConfigItemCurrent object cannot handle that on its own, so we set the current back to the keyboard which has been renamed.
                    RefreshKeyboardValues(s,e);
                };
                _app.KeyboardContext.Keyboards.CurrentChanged += ( s, e ) => { _keyboards.RefreshCurrent( s, e ); };
            }

            var g = this.AddGroup();
            var skinStarter = new ConfigFeatureStarter( ConfigManager, _app.PluginRunner, _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _skinId ) { DisplayName = R.SkinSectionName };
            var i = new ConfigFeatureStarter( ConfigManager, _app.PluginRunner, _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _autoclicId ) { DisplayName = R.AutoClickSectionName };
            g.Items.Add( skinStarter );
            g.Items.Add( i );

            this.AddLink( _appConfigVm ?? ( _appConfigVm = new AppConfigViewModel( _app ) ) );
            base.OnInitialize();
        }

        public CivikeyStandardHost CivikeyHost { get; private set; }

        

    }
}
