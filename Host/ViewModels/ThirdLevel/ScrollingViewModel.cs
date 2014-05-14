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
