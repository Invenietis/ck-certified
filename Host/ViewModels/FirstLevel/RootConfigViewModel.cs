#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\FirstLevel\RootConfigViewModel.cs) is part of CiviKey. 
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
using CK.Plugin.Config;
using Host.Resources;
using CK.Windows.Config;
using Host.VM;
using Host.ViewModels;

namespace Host
{
    //First level of the civikey host
    public class RootConfigViewModel : ConfigPage
    {
        readonly Guid _screenScrollerId;
        readonly Guid _clickSelectorId;
        readonly Guid _basicScrollId;
        readonly Guid _autoclicId;
        readonly Guid _radarId;
        readonly Guid _skinId;

        readonly AppViewModel _app;
        AppConfigViewModel _appConfigVm;
        ConfigItemCurrent<KeyboardModel> _keyboards;

        public RootConfigViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.Home;
            _app = app;
            _screenScrollerId = new Guid( "{AE25D80B-B927-487E-9274-48362AF95FC0}" );
            _clickSelectorId = new Guid( "{F9687F04-7370-4812-9EB4-1320EB282DD8}" );
            _basicScrollId = new Guid( "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}" );
            _autoclicId = new Guid( "{989BE0E6-D710-489e-918F-FBB8700E2BB2}" );
            _radarId = new Guid( "{390AFE83-C5A2-4733-B5BC-5F680ABD0111}" );
            _skinId = new Guid( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" );
        }

        protected override void OnInitialize()
        {
            _app.PluginRunner.IsDirtyChanged += OnPluginRunnerDirtyChanged;

            if( _app.KeyboardContext != null )
            {
                var ctxModel = new ContextModel( _app );

                _keyboards = this.AddCurrentItem( R.Keyboard, null, ctxModel, ctx => ctx.Current, ctx => ctx.Keyboards, false, String.Empty );
                _keyboards.ImagePath = "pack://application:,,,/CiviKey;component/Views/Images/Keyboard.png";
            }

            var skinStarter = new ConfigFeatureStarter( ConfigManager, _app.PluginRunner, new PluginCluster( _app.PluginRunner, _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _skinId ) ) { DisplayName = R.SkinSectionName };
            var autoClicStarter = new ConfigFeatureStarter( ConfigManager, _app.PluginRunner, new PluginCluster( _app.PluginRunner, _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _autoclicId, _clickSelectorId ) ) { DisplayName = R.AutoClickSectionName };
            var basicScrollStarter = new ConfigFeatureStarter( ConfigManager, _app.PluginRunner, new PluginCluster( _app.PluginRunner, _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _basicScrollId, new Guid[0], new Guid[] { _radarId, _screenScrollerId } ) ) { DisplayName = R.Scrolling };

            var wordPredictionStarter = new ConfigFeatureStarter( ConfigManager, _app.PluginRunner, new PluginCluster( _app.PluginRunner, _app.CivikeyHost.Context.ConfigManager.UserConfiguration,
                new Guid( "{1756C34D-EF4F-45DA-9224-1232E96964D2}" ), //InKeyboardWordPredictor
                new Guid( "{1764F522-A9E9-40E5-B821-25E12D10DC65}" ), //SybilleWordPredictorService
                new Guid( "{669622D4-4E7E-4CCE-96B1-6189DC5CD5D6}" ), //WordPredictedService
                new Guid( "{4DC42B82-4B29-4896-A548-3086AA9421D7}" ), //WordPredictorFeature
                new Guid( "{8789CDCC-A7BB-46E5-B119-28DC48C9A8B3}" ), //SimplePredictedWordSender
                new Guid( "{69E910CC-C51B-4B80-86D3-E86B6C668C61}" ), //TextualContextArea
                new Guid( "{86777945-654D-4A56-B301-5E92B498A685}" ), //TextualContextService
                new Guid( "{B2A76BF2-E9D2-4B0B-ABD4-270958E17DA0}" ), //TextualContextCommandHandler
                new Guid( "{55C2A080-30EB-4CC6-B602-FCBBF97C8BA5}" )  //PredictionTextAreaBus
                ) ) { DisplayName = R.WordPredictionSectionName };

            var g = this.AddGroup();
            g.Items.Add( skinStarter );
            g.Items.Add( autoClicStarter );
            g.Items.Add( wordPredictionStarter );
            g.Items.Add( basicScrollStarter );

            this.AddLink( new ImplementationSelector( R.MouseScrollingSelection, _app ) );
            this.AddLink( _appConfigVm ?? (_appConfigVm = new AppConfigViewModel( _app )) );

            base.OnInitialize();
        }

        public void OnPluginRunnerDirtyChanged( object sender, EventArgs e )
        {
            if( _app.PluginRunner.IsDirty && !_app.PluginRunner.Disabled )
                _app.PluginRunner.Apply();
        }

        public void StartObjectExplorer()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{4BF2616D-ED41-4E9F-BB60-72661D71D4AF}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }
    }
}
