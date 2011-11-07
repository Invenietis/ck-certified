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
        string _packageName; //Can be Civikey-Standard or Civikey-Steria etc...

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

        public Version NewVersion  { get; private set; }

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
            _packageName = (string)Configuration.System["PackageName"] ?? HostInformation.AppName;

            UpdateVersionState savedState = _versionState;
            VersionState = UpdateVersionState.CheckingForNewVersion;           
            _webClient.DownloadDataAsync( new Uri( GetServerUrl() + "version/updated/currentversion/" + _packageName ), savedState );//gets the new version from its package repository
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
                    if( NewVersion > HostInformation.AppVersion )
                    {
                        VersionState = UpdateVersionState.NewerVersionAvailable;
                        // Ask the user to download it.
                        OnNewerVersionAvailable();
                    }
                    else
                    {
                        VersionState = UpdateVersionState.NoNewerVersion;
                    }
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
            _webClient.DownloadFileAsync( new Uri( GetServerUrl() + "version/updated/download/" + _packageName ), _downloading.Path, savedState );
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
                    string newVersionDir = Path.Combine( Path.GetTempPath(), HostInformation.AppName + @"\Updates\" ); //puts the exe in Temp/CiviKey/Updates
                    Directory.CreateDirectory( newVersionDir );
                    File.Move( _downloading.Path, Path.Combine( newVersionDir, "Update.exe" ) );
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
            MessageBox.Show( "Une mise à jour est prète à être installée.\nElle vous sera proposé au prochain lancement de Civikey.", "Mise à jour" );
        }
    }
}
