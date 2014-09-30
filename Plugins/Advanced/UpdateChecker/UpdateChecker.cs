#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\UpdateChecker\UpdateChecker.cs) is part of CiviKey. 
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Windows;
using CK.Context;
using CK.Context.SemVer;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows.App;
using CommonServices;
using Host.Services;
using UpdateChecker.View;

namespace UpdateChecker
{
    /// <summary>
    /// This plugin can interact with the user. It is not ideal but since its interactions are minimals (message box),
    /// this avoid the creation of an associated GUI plugin.
    /// </summary>
    [Plugin( PluginGuidString, Version = PluginVersion, PublicName = PluginPublicName )]
    public class UpdateChecker : IPlugin, IUpdateChecker
    {
        #region Plugin description

        const string PluginGuidString = "{11C83441-6818-4A8B-97A0-1761E1A54251}";
        const string PluginVersion = "2.0.0";
        const string PluginPublicName = "Update Checker";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        IActivityMonitor _log;

        UpdateVersionState _versionState;
        DownloadState _downloadState;
        WebClient _webClient;
        TemporaryFile _downloading;
        IDisposable _downloadingNotificationHandler;

        [RequiredService]
        public INotificationService Notifications { get; set; }

        [RequiredService]
        public IHostInformation HostInformation { get; set; }

        public IPluginConfigAccessor Configuration { get; set; }

        public SemanticVersion20 NewVersion { get; private set; }

        bool? _shouldIncludePrerelease;
        public bool ShouldIncludePrerelease
        {
            get
            {
                if( !_shouldIncludePrerelease.HasValue )
                    _shouldIncludePrerelease = (bool)Configuration.System.GetOrSet<bool>( "ShouldIncludePrerelease", false );

                return _shouldIncludePrerelease.Value;
            }
            set
            {
                _shouldIncludePrerelease = value;
                Configuration.System["ShouldIncludePrerelease"] = value;
            }
        }

        public UpdateVersionState VersionState
        {
            get { return _versionState; }
            set
            {
                if( _versionState != value )
                {
                    _versionState = value;
                    if( StateChanged != null ) StateChanged( this, EventArgs.Empty );
                }
            }
        }

        public DownloadState DownloadState
        {
            get { return _downloadState; }
            set
            {
                if( _downloadState != value )
                {
                    _downloadState = value;
                    if( StateChanged != null ) StateChanged( this, EventArgs.Empty );
                }
            }
        }

        public event EventHandler StateChanged;

        public bool Setup( IPluginSetupInfo info )
        {
            _log = new ActivityMonitor( "UpdateChecker" );

            _versionState = UpdateVersionState.Unknown;
            _downloadState = DownloadState.None;
            _webClient = new WebClient();
            // Used to read the available version.
            _webClient.DownloadDataCompleted += new DownloadDataCompletedEventHandler( OnDownloadDataCompleted );
            // Used to download the new version.
            _webClient.DownloadFileCompleted += new AsyncCompletedEventHandler( OnDownloadFileCompleted );
            return true;
        }

        public void Start()
        {
            CheckForUpdate();
        }

        public void Stop()
        {
            if( _webClient.IsBusy ) _webClient.CancelAsync();
        }

        public void Teardown()
        {
            _webClient.Dispose();
            if( _downloading != null ) _downloading.Dispose();
        }

        string GetCheckUpdateUrl( string distributionName )
        {
            string url = GetServerUrl() + "v2/update/" + distributionName;
            if( ShouldIncludePrerelease )
                url += "?includeprerelease=" + ShouldIncludePrerelease.ToString();

            return url;
        }

        string GetReleaseNoteUrl( string distributionName, SemanticVersion20 version )
        {
            return GetServerUrl() + string.Format( "v2/update/{0}/{1}/release-notes", distributionName, version.ToString() );
        }

        string GetDownloadUrl( string distributionName, SemanticVersion20 version )
        {
            return GetServerUrl() + string.Format( "v2/update/{0}/{1}/installer", distributionName, version.ToString() );
        }

        private string GetServerUrl()
        {
            // Gets the UpdateServerUrl now (to correctly handle configuration changes).
            string server = Configuration.System.GetOrSet( "UpdateServerUrl", "http://api.civikey.invenietis.com/" );
            if( !server.EndsWith( "/" ) ) server += "/";
            return server;
        }

        private void CheckNotBusy()
        {
            if( _webClient.IsBusy ) throw new InvalidOperationException();
            Debug.Assert( _versionState != UpdateVersionState.CheckingForNewVersion && _downloadState != DownloadState.Downloading );
        }

        public bool IsBusy
        {
            get { return _webClient.IsBusy; }
        }

        public void CheckForUpdate()
        {
            CheckNotBusy();

            UpdateVersionState savedState = _versionState;
            VersionState = UpdateVersionState.CheckingForNewVersion;
            string httpRequest = GetCheckUpdateUrl( HostInformation.SubAppName );

            _webClient.DownloadDataAsync( new Uri( httpRequest ), savedState );
        }

        void StartDownloadReleaseNotes()
        {
            VersionState = UpdateVersionState.DownloadingReleaseNotes;
            _webClient.DownloadDataAsync( new Uri( GetReleaseNoteUrl( HostInformation.SubAppName, NewVersion ) ) );
        }

        void OnDownloadDataCompleted( object sender, DownloadDataCompletedEventArgs e )
        {
            if( e.Cancelled )
            {
                VersionState = (UpdateVersionState)e.UserState;
            }
            else
            {
                Debug.Assert( _versionState == UpdateVersionState.CheckingForNewVersion || _versionState == UpdateVersionState.DownloadingReleaseNotes );

                try
                {
                    if( _versionState == UpdateVersionState.CheckingForNewVersion )
                    {
                        if( e.Error != null )
                        {
                            _log.Error().Send( e.Error, "Error while checking version" );
                            VersionState = UpdateVersionState.ErrorWhileCheckingVersion;
                            return;
                        }

                        string version = Encoding.UTF8.GetString( e.Result ).Replace( "\"", string.Empty );
                        NewVersion = SemanticVersion20.Parse( version );
                        if( NewVersion > SemanticVersion20.Parse( HostInformation.AppVersion.ToString() ) )//If the version retrieved from the server is greater than the currently installed one
                        {
                            // If the version retrived from the server is greater than the one that has been downloaded last
                            string retrievedVersionString = Configuration.System.GetOrSet<string>( "LastDownloadedVersion", "0.0.0" );
                            SemanticVersion20 retrievedVersion;
                            if( SemanticVersion20.TryParse( retrievedVersionString, out retrievedVersion ) && retrievedVersion < NewVersion )
                            {
                                // if a new version is available, try to download release notes before we show the update to the user.
                                StartDownloadReleaseNotes();
                            }
                        }
                        else
                            VersionState = UpdateVersionState.NoNewerVersion;
                    }
                    else if( _versionState == UpdateVersionState.DownloadingReleaseNotes )
                    {
                        VersionState = UpdateVersionState.NewerVersionAvailable;
                        string rn = null;
                        if( e.Error == null )
                        {
                            rn = Encoding.UTF7.GetString( e.Result );
                        }
                        // Ask the user to download it.
                        Application.Current.Dispatcher.BeginInvoke( (Action)(() =>
                        {
                            OnNewerVersionAvailable( rn );
                        }) );
                    }
                }
                catch( Exception ex )
                {
                    _log.Error().Send( ex, "Error while checking version" );
                    VersionState = UpdateVersionState.ErrorWhileCheckingVersion;
                }
            }
        }

        public void StartDownload()
        {
            CheckNotBusy();
            DownloadState savedState = _downloadState;
            _downloading = new TemporaryFile();
            DownloadState = DownloadState.Downloading;
            string httpRequest = GetDownloadUrl( HostInformation.SubAppName, NewVersion );
            _webClient.DownloadFileAsync( new Uri( httpRequest ), _downloading.Path, savedState );

            if( Notifications != null )
                _downloadingNotificationHandler = Notifications.ShowNotification( PluginId.UniqueId, "Update in progress", "CiviKey is downloading its new version.", 0, NotificationTypes.Message );
        }

        void OnDownloadFileCompleted( object sender, AsyncCompletedEventArgs e )
        {
            if( _downloadingNotificationHandler != null )
                _downloadingNotificationHandler.Dispose();

            if( e.Cancelled )
            {
                DownloadState = (DownloadState)e.UserState;
            }
            else if( e.Error != null )
            {
                _log.Error().Send( e.Error, "Error while downloading" );
                DownloadState = DownloadState.ErrorWhileDownloading;
            }
            else
            {
                Debug.Assert( _downloadState == DownloadState.Downloading );
                try
                {
                    string newVersionDir = Path.Combine( HostInformation.CommonApplicationDataPath, "Updates" ); //puts the exe in ProgramData/Appname/DistributionName/Updates
                    Directory.CreateDirectory( newVersionDir );
                    string updateFilePath = Path.Combine( newVersionDir, "Update.exe" );
                    if( File.Exists( updateFilePath ) ) File.Delete( updateFilePath );

                    File.Move( _downloading.Path, updateFilePath );

                    //Giving read/write/execute rights to any user on the Update.exe file.
                    FileSecurity f = File.GetAccessControl( updateFilePath );
                    var sid = new SecurityIdentifier( WellKnownSidType.BuiltinUsersSid, null );
                    NTAccount account = (NTAccount)sid.Translate( typeof( NTAccount ) );
                    f.AddAccessRule( new FileSystemAccessRule( account, FileSystemRights.Modify, AccessControlType.Allow ) );
                    File.SetAccessControl( updateFilePath, f );

                    //Setting the version of the downloaded file in the User Configuration, so that we do not have to download it again during the next launch
                    Configuration.System.Set( "LastDownloadedVersion", NewVersion.ToString() );

                    _downloading.Dispose();
                    _downloading = null;
                    DownloadState = DownloadState.Downloaded;
                    // Warn the user that the download is available.
                    OnNewerVersionDownloaded();
                }
                catch( Exception ex )
                {
                    _log.Error().Send( ex, "Error while downloading" );
                    DownloadState = DownloadState.ErrorWhileDownloading;
                }
            }
        }

        void OnNewerVersionAvailable( string releaseNotes )
        {
            var wnd = new NewerVersionView();
            var vm = new NewerVersionViewModel( releaseNotes, NewVersion.ToString(), r =>
            {
                wnd.DialogResult = r;
                wnd.Close();
            }, s => wnd.SetBrowserContent( s ) );

            wnd.DataContext = vm;

            bool? shouldStartDownload = wnd.ShowDialog();
            if( shouldStartDownload.HasValue && shouldStartDownload.Value )
                StartDownload();
        }

        void OnNewerVersionDownloaded()
        {
            ModalViewModel mvm = new ModalViewModel( R.UpdateDownloadedTitle, R.UpdateDownloadedContent );
            mvm.Buttons.Add( new ModalButton( mvm, R.Ok, null, ModalResult.Ok ) );

            CustomMsgBox msg = new CustomMsgBox( ref mvm );
            msg.ShowDialog();
        }
    }
}
