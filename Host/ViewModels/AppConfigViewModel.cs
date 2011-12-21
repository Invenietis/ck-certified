using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using CK.WPF.Controls;
using CK.Plugin.Config;
using CK.Core;
using CK.Reflection;
using Host.Resources;
using System.ComponentModel;
using CK.Windows.Config;
using CK.Windows;

namespace Host.VM
{
    public class AppConfigViewModel : ConfigPage
    {
        AppViewModel _app;
        AutoClickViewModel _acVm;
        SkinViewModel _sVm;

        public AppConfigViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.AppConfig;
            _app = app;
        }

        protected override void OnInitialize()
        {
            var profiles = this.AddProperty( R.Profile, _app.CivikeyHost.Context.ConfigManager.SystemConfiguration, a => a.UserProfiles );
            _app.CivikeyHost.Context.ConfigManager.SystemConfiguration.UserProfiles.CollectionChanged += profiles.ValueRefresh;

            var g = this.AddGroup();
            g.AddProperty( R.ShowTaskbarIcon, _app, a => a.ShowTaskbarIcon );
            g.AddProperty( R.ShowSystrayIcon, _app, a => a.ShowSystrayIcon );

            //this.AddLink( _acVm ?? (_acVm = new AutoClickViewModel( _app )) );
            //this.AddLink( _sVm ?? (_sVm = new SkinViewModel( _app )) );
            
            var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartEditor ) );
            action.ImagePath = "edit.png";
            action.DisplayName = R.SkinViewConfig;
            action.Description = R.AdvancedUserNotice;
            this.Items.Add( action );

            base.OnInitialize();
        }

        public void StartEditor()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{402C9FF7-545A-4E3C-AD35-70ED37497805}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }

    }
}
