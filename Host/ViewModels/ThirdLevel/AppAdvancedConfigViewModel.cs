#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\ThirdLevel\AppAdvancedConfigViewModel.cs) is part of CiviKey. 
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

using Host.Resources;
using CK.Windows.Config;
using System.IO;
using System.IO.Compression;
using System.Security.Principal;
using System.Security.AccessControl;
using CK.Windows;
using CK.Plugin.Config;
using System;
using Ionic.Zip;

namespace Host.VM
{
    public class AppAdvancedConfigViewModel : ConfigPage
    {
        string _stopReminderPath;
        AppViewModel _app;

        public AppAdvancedConfigViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.AdvancedAppConfig;
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
            g.AddProperty( R.RemindMeOfNewUpdates, this, a => a.RemindMeOfNewUpdates );

            {
                var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( ExportConf ) );
                action.ImagePath = "Forward.png";
                action.DisplayName = R.ExportConf;
                this.Items.Add( action );
            }

            // v2.7.0 : the notificationmanager has been removed, so we don't have a systray icon anymore. Put this back on together with the notification manager.
            //g.AddProperty( R.ShowSystrayIcon, _app, a => a.ShowSystrayIcon );

            //TODOJL
            //this.AddAction( "Check online help contents", _app.HelpUpdaterService.StartManualUpdate );

            string stopReminderFolderPath = Path.Combine( _app.CivikeyHost.ApplicationDataPath, "Updates" );
            if( !Directory.Exists( stopReminderFolderPath ) ) Directory.CreateDirectory( stopReminderFolderPath );
            _stopReminderPath = Path.Combine( stopReminderFolderPath, "StopReminder" );
            _remindMeOfNewUpdates = !File.Exists( _stopReminderPath );

            base.OnInitialize();
        }

        public void ExportConf()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            DateTime now = DateTime.Now;
            dlg.FileName = string.Format( "Configuration - {0:ddMMyy}", now ); // Default file name
            dlg.DefaultExt = ".zip"; // Default file extension
            dlg.Filter = "Configuration CiviKey (.zip)|*.zip"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if( result == true )
            {
                ZipMultiFiles( dlg.FileName, CivikeyStandardHost.Instance.ApplicationDataPath + "Context.xml", 
                                             CivikeyStandardHost.Instance.ApplicationDataPath + "User.config.ck", 
                                             CivikeyStandardHost.Instance.CommonApplicationDataPath + "System.config.ck" );
            }
        }

        public static void ZipMultiFiles( string destPath, params string[] filePaths )
        {
            using( ZipFile z = new ZipFile() )
            {
                z.AddFiles( filePaths, string.Empty );
                z.Save( destPath );
            }
            
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
