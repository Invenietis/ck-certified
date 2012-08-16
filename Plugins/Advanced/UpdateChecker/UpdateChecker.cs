using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using CK.Context;
using CK.Plugin;
using Host.Services;
using CommonServices;
using CK.Core;
using CK.Plugin.Config;
using System.Collections.Generic;
using Host;
using System.Text;
using System.Security.AccessControl;
using System.Security.Principal;

namespace UpdateChecker
{
    /// <summary>
    /// This plugin can interact with the user. It is not ideal but since its interactions are minimals (message box),
    /// this avoid the creation of an associated GUI plugin.
    /// </summary>
    [Plugin( UpdateChecker.PluginIdentifier, Version = UpdateChecker.PluginVersion, PublicName = "Update Checker" )]
    public class UpdateChecker : IPlugin, IUpdateChecker
    {
        const string PluginIdentifier = "{11C83441-6818-4A8B-97A0-1761E1A54251}";
        const string PluginVersion = "1.0.0";
        string _distributionName; //Can be "Standard" or "Steria" etc...

        UpdateVersionState _versionState;
        UpdateDownloadState _downloadState;
        WebClient _webClient;
        TemporaryFile _downloading;
        IDisposable _downloadingNotificationHandler;
        static Common.Logging.ILog _log = Common.Logging.LogManager.GetLogger<UpdateChecker>();

        [RequiredService]
        public INotificationService Notifications { get; set; }

        [RequiredService]
        public IHostInformation HostInformation { get; set; }

        public IPluginConfigAccessor Configuration { get; set; }

        public Version NewVersion { get; private set; }

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

        public UpdateDownloadState DownloadState
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
            _versionState = UpdateVersionState.Unknown;
            _downloadState = UpdateDownloadState.None;
            _webClient = new WebClient();
            // Used to read the available version.
            _webClient.DownloadDataCompleted += new DownloadDataCompletedEventHandler( _webClient_DownloadDataCompleted );
            // Used to download the new version.
            _webClient.DownloadFileCompleted += new AsyncCompletedEventHandler( _webClient_DownloadFileCompleted );
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

        private string GetServerUrl()
        {
            // Gets the UpdateServerUrl now (to correctly handle configuration changes).
            string server = (string)Configuration.System["UpdateServerUrl"] ?? "http://releases.civikey.invenietis.com/";
            if( !server.EndsWith( "/" ) ) server += "/";
            return server;
        }

        private void CheckNotBusy()
        {
            if( _webClient.IsBusy ) throw new InvalidOperationException();
            Debug.Assert( _versionState != UpdateVersionState.CheckingForNewVersion && _downloadState != UpdateDownloadState.Downloading );
        }

        public bool IsBusy
        {
            get { return _webClient.IsBusy; }
        }

        public void CheckForUpdate()
        {
            CheckNotBusy();
            _distributionName = (string)Configuration.System["DistributionName"] ?? HostInformation.SubAppName;

            UpdateVersionState savedState = _versionState;
            VersionState = UpdateVersionState.CheckingForNewVersion;
            string httpRequest = GetServerUrl() + "version/updated/currentversion/" + HostInformation.AppName + @"-" + _distributionName + "/" + HostInformation.AppVersion.ToString();
            _webClient.DownloadDataAsync( new Uri( httpRequest ), savedState );//gets the new version from its package repository
        }

        void _webClient_DownloadDataCompleted( object sender, DownloadDataCompletedEventArgs e )
        {
            if( e.Cancelled )
            {
                VersionState = (UpdateVersionState)e.UserState;
            }
            else if( e.Error != null )
            {
                _log.Error( "Error while checking version", e.Error );
                VersionState = UpdateVersionState.ErrorWhileCheckingVersion;
            }
            else
            {
                Debug.Assert( _versionState == UpdateVersionState.CheckingForNewVersion );
                try
                {
                    string version = Encoding.UTF8.GetString( e.Result );
                    NewVersion = new Version( version );
                    if( NewVersion > HostInformation.AppVersion //If the version retrieved from the server is greater than the currently installed one
                        && Configuration.System.GetOrSet<Version>( "LastDownloadedVersion", new Version( "0.0.0" ) ) < NewVersion ) //If the version retrived from the server is greater than the one that has been downloaded last
                    {
                        VersionState = UpdateVersionState.NewerVersionAvailable;
                        // Ask the user to download it.
                        OnNewerVersionAvailable();
                    }

                    VersionState = UpdateVersionState.NoNewerVersion;
                }
                catch( Exception ex )
                {
                    _log.Error( "Error while checking version", ex );
                    VersionState = UpdateVersionState.ErrorWhileCheckingVersion;
                }
            }
        }

        public void StartDownload()
        {
            CheckNotBusy();
            UpdateDownloadState savedState = _downloadState;
            _downloading = new TemporaryFile();
            DownloadState = UpdateDownloadState.Downloading;
            string httpRequest = GetServerUrl() + "version/updated/download/" + Path.Combine( HostInformation.AppName + @"-" + _distributionName, HostInformation.AppVersion.ToString() );
            _webClient.DownloadFileAsync( new Uri( httpRequest ), _downloading.Path, savedState );
            _downloadingNotificationHandler = Notifications.ShowNotification( new Guid( PluginIdentifier ), "Update in progress", "CiviKey is downloading its new version.", 0, NotificationTypes.Message );
        }

        void _webClient_DownloadFileCompleted( object sender, AsyncCompletedEventArgs e )
        {
            if( _downloadingNotificationHandler != null )
                _downloadingNotificationHandler.Dispose();

            if( e.Cancelled )
            {
                DownloadState = (UpdateDownloadState)e.UserState;
            }
            else if( e.Error != null )
            {
                _log.Error( "Error while downloading", e.Error );
                DownloadState = UpdateDownloadState.ErrorWhileDownloading;
            }
            else
            {
                Debug.Assert( _downloadState == UpdateDownloadState.Downloading );
                try
                {
                    string newVersionDir = Path.Combine( HostInformation.CommonApplicationDataPath, "Updates" ); //puts the exe in ProgramData/Appname/DistributionName/Updates
                    Directory.CreateDirectory( newVersionDir );
                    string updateFilePath = Path.Combine( newVersionDir, "Update.exe" );
                    File.Move( _downloading.Path, updateFilePath );

                    //Giving read/write/execute rights to any user on the Update.exe file.
                    FileSecurity f = File.GetAccessControl( updateFilePath );
                    var sid = new SecurityIdentifier( WellKnownSidType.BuiltinUsersSid, null );
                    NTAccount account = (NTAccount)sid.Translate( typeof( NTAccount ) );
                    f.AddAccessRule( new FileSystemAccessRule( account, FileSystemRights.Modify, AccessControlType.Allow ) );
                    File.SetAccessControl( updateFilePath, f );

                    //Setting the version of the downloaded file in the User Configuration, so that we do not have to download it again during the next launch
                    Configuration.System.Set( "LastDownloadedVersion", NewVersion );

                    _downloading.Dispose();
                    _downloading = null;
                    DownloadState = UpdateDownloadState.Downloaded;
                    // Warn the user that the download is available.
                    OnNewerVersionDownloaded();
                }
                catch( Exception ex )
                {
                    _log.Error( "Error while downloading", ex );
                    DownloadState = UpdateDownloadState.ErrorWhileDownloading;
                }
            }
        }

        private void OnNewerVersionAvailable()
        {
            if( MessageBox.Show( String.Format( "Une mise à jour est disponible (version {0}).\nVoulez-vous la télécharger ?", NewVersion ), "Mise à jour", MessageBoxButton.YesNo ) == MessageBoxResult.Yes )
            {
                StartDownload();
            }
        }

        private void OnNewerVersionDownloaded()
        {
            MessageBox.Show( "Une mise à jour est prête à être installée.\nElle vous sera proposée au prochain lancement de Civikey.", "Mise à jour" );
        }
    }
}
