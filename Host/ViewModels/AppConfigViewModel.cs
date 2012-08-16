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
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Security.Principal;
using System.Security.AccessControl;

namespace Host.VM
{
    public class AppConfigViewModel : ConfigPage
    {
        string _stopReminderPath;
        AutoClickViewModel _acVm;
        SkinViewModel _sVm;
        AppViewModel _app;

        public AppConfigViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.AppConfig;
            _app = app;
        }

        protected override void OnInitialize()
        {
            var profiles = this.AddCurrentItem( R.Profile, "", _app.CivikeyHost.Context.ConfigManager.SystemConfiguration, a => a.CurrentUserProfile, a => a.UserProfiles, false, "" );
            _app.CivikeyHost.Context.ConfigManager.SystemConfiguration.UserProfiles.CollectionChanged += ( s, e ) =>
            {
                profiles.RefreshValues( s, e );
            };

            var g = this.AddGroup();
            g.AddProperty( R.ShowTaskbarIcon, _app, a => a.ShowTaskbarIcon );
            g.AddProperty( R.ShowSystrayIcon, _app, a => a.ShowSystrayIcon );
            g.AddProperty( R.RemindMeOfNewUpdates, this, a => a.RemindMeOfNewUpdates );

            this.AddLink( _acVm ?? ( _acVm = new AutoClickViewModel( _app ) ) );
            this.AddLink( _sVm ?? ( _sVm = new SkinViewModel( _app ) ) );

            string stopReminderFolderPath = Path.Combine( _app.CivikeyHost.ApplicationDataPath, "Updates" );
            if( !Directory.Exists( stopReminderFolderPath ) ) Directory.CreateDirectory( stopReminderFolderPath );
            _stopReminderPath = Path.Combine( stopReminderFolderPath, "StopReminder" );
            _remindMeOfNewUpdates = !File.Exists( _stopReminderPath );

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

        bool _remindMeOfNewUpdates;
        public bool RemindMeOfNewUpdates
        {
            get { return _remindMeOfNewUpdates; }
            set
            {
                if( value )
                {
                    File.Delete( _stopReminderPath );
                }

                else
                {
                    File.Create( _stopReminderPath ).Close();
                    FileSecurity f = File.GetAccessControl( _stopReminderPath );
                    var sid = new SecurityIdentifier( WellKnownSidType.BuiltinUsersSid, null );
                    NTAccount account = (NTAccount)sid.Translate( typeof( NTAccount ) );
                    f.AddAccessRule( new FileSystemAccessRule( account, FileSystemRights.Modify, AccessControlType.Allow ) );
                    File.SetAccessControl( _stopReminderPath, f );
                }

                _remindMeOfNewUpdates = value;
                NotifyOfPropertyChange( () => RemindMeOfNewUpdates );
            }
        }
    }
}
