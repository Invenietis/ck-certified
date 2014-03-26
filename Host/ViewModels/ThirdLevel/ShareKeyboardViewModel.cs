using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Config;
using CK.Windows;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    public class ShareKeyboardViewModel : ConfigPage
    {
        AppViewModel _app;

        public ShareKeyboardViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.ShareKeyboard;
            _app = app;
        }

        protected override void OnInitialize()
        {
            var profiles = this.AddCurrentItem( R.Profile, "", _app.CivikeyHost.Context.ConfigManager.SystemConfiguration, a => a.CurrentUserProfile, a => a.UserProfiles, false, "" );
            _app.CivikeyHost.Context.ConfigManager.SystemConfiguration.UserProfiles.CollectionChanged += ( s, e ) =>
            {
                profiles.RefreshValues( s, e );
            };

            {
                var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartImport ) );
                action.ImagePath = "Forward.png";
                action.DisplayName = R.ImportKeyboard;
                this.Items.Add( action );
            }

            {
                var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartExport) );
                action.ImagePath = "Forward.png";
                action.DisplayName = R.ExportKeyboard;
                this.Items.Add( action );
            }

            base.OnInitialize();
        }

        public void StartImport()
        {

            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{D94D1757-5BFB-4B80-9C8E-1B108F5C7086}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }

        public void StartExport()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{244C578B-322A-4733-A34B-EEC0558F61D5}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }
    }
}
