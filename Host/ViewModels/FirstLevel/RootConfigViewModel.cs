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
using Host.ViewModels;

namespace Host
{
    //First level of the civikey host
    public class RootConfigViewModel : CK.Windows.Config.ConfigPage
    {
        AppViewModel _app;
        AppConfigViewModel _appConfigVm;
        Guid _autoclicId;
        Guid _skinId;
        Guid _basicScrollId;
        ConfigItemCurrent<KeyboardModel> _keyboards;

        public RootConfigViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = Resources.R.Home;
            _app = app;
            _autoclicId = new Guid( "{989BE0E6-D710-489e-918F-FBB8700E2BB2}" );
            _skinId = new Guid( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" );
            _basicScrollId = new Guid( "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}" );
        }

        protected override void OnInitialize()
        {
            if( _app.KeyboardContext != null )
            {
                ContextModel ctxModel = new ContextModel( _app.KeyboardContext );

                _keyboards = this.AddCurrentItem( R.Keyboard, null, ctxModel, ctx => ctx.Current, ctx => ctx.Keyboards, false, "" );
                _keyboards.ImagePath = "/Views/Images/Keyboard.png";//"pack://application:,,,/CK-Certified;component/Views/Images/Keyboard.png"
            }

            var g = this.AddGroup();
            var skinStarter = new ConfigFeatureStarter( ConfigManager, _app.PluginRunner, _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _skinId ) { DisplayName = R.SkinSectionName };
            var autoClicStarter = new ConfigFeatureStarter( ConfigManager, _app.PluginRunner, _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _autoclicId ) { DisplayName = R.AutoClickSectionName };
            var basicScrollStarter = new ConfigFeatureStarter( ConfigManager, _app.PluginRunner, _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _basicScrollId,
                                                new Guid( "{4EDBED5A-C38E-4A94-AD34-18720B09F3B7}" ),
                                                new Guid( "{B2EC4D13-7A4F-4F9E-A713-D5F8DDD161EF}" ),
                                                new Guid( "{4A3F1565-E127-473c-B169-0022A3EDB58D}" ) ) { DisplayName = "Défilement clavier" };

            var wordPredictionStarter = new ConfigFeatureStarter( ConfigManager, _app.PluginRunner, _app.CivikeyHost.Context.ConfigManager.UserConfiguration,
                new Guid( "{1756C34D-EF4F-45DA-9224-1232E96964D2}" ), //InKeyboardWordPredictor
                new Guid( "{1764F522-A9E9-40E5-B821-25E12D10DC65}" ), // SybilleWordPredictorService
                new Guid( "{669622D4-4E7E-4CCE-96B1-6189DC5CD5D6}" ), // WordPredictedService
                new Guid( "{4DC42B82-4B29-4896-A548-3086AA9421D7}" ), //WordPredictorFeature
                new Guid( "{8789CDCC-A7BB-46E5-B119-28DC48C9A8B3}" ), //SimplePredictedWordSender
                new Guid( "{69E910CC-C51B-4B80-86D3-E86B6C668C61}" ), //TextualContextArea
                new Guid( "{86777945-654D-4A56-B301-5E92B498A685}" ), //TextualContextService
                new Guid( "{B2A76BF2-E9D2-4B0B-ABD4-270958E17DA0}" ), //TextualContextCommandHandler
                new Guid( "{55C2A080-30EB-4CC6-B602-FCBBF97C8BA5}" ) //PredictionTextAreaBus
                )
            {
                DisplayName = R.WordPredictionSectionName
            };
            g.Items.Add( skinStarter );
            g.Items.Add( autoClicStarter );
            g.Items.Add( wordPredictionStarter );
            g.Items.Add( basicScrollStarter );

            this.AddLink( _appConfigVm ?? ( _appConfigVm = new AppConfigViewModel( _app ) ) );

            base.OnInitialize();
        }

        public CivikeyStandardHost CivikeyHost { get; private set; }


        public void StartObjectExplorer()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{4BF2616D-ED41-4E9F-BB60-72661D71D4AF}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }

    }
}
