using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using CK.Core;
using Help.Services;

namespace Help.Update.ManualUpdate
{
    public class MainViewModel : PropertyChangedBase
    {
        IHelpUpdaterService _updaterService;
        Action<INamedVersionedUniqueId, IDownloadResult> _manualInstaller;

        public MainViewModel( IHelpUpdaterService updaterService, Action<INamedVersionedUniqueId, IDownloadResult> manualInstaller )
        {
            _updaterService = updaterService;
            _manualInstaller = manualInstaller;

            _updaterService.UpdateAvailable += OnUpdateAvailable;
            _updaterService.UpdateDownloaded += OnUpdateDownloaded;
            _updaterService.UpdateInstalled += OnUpdateInstalled;

            PluginUpdates = new ObservableCollection<PluginUpdateViewModel>();
        }

        public ObservableCollection<PluginUpdateViewModel> PluginUpdates { get; private set; }

        bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if( _isBusy != value )
                {
                    _isBusy = value;
                    NotifyOfPropertyChange( () => IsBusy );
                    NotifyOfPropertyChange( () => IsEmptyMessageVisibility );
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                return !IsBusy && PluginUpdates.Count == 0;
            }
        }

        public Visibility IsEmptyMessageVisibility
        {
            get
            {
                return IsEmpty ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        void OnUpdateAvailable( object sender, HelpUpdateEventArgs e )
        {
            var vm = FindOrCreateVm( e.Plugin );
            vm.State = PluginUpdateState.Downloading;
        }

        void OnUpdateDownloaded( object sender, HelpUpdateDownloadedEventArgs e )
        {
            var vm = FindOrCreateVm( e.Plugin );
            vm.State = PluginUpdateState.WaitingForInstall;
            vm.DownloadResult = e.DownloadResult;
        }

        void OnUpdateInstalled( object sender, HelpUpdateDownloadedEventArgs e )
        {
            var vm = FindOrCreateVm( e.Plugin );
            vm.State = PluginUpdateState.Installed;
        }

        PluginUpdateViewModel FindOrCreateVm( INamedVersionedUniqueId pluginId )
        {
            var plugin = PluginUpdates.FirstOrDefault( p => p.PluginId == pluginId );
            if( plugin == null )
            {
                plugin = new PluginUpdateViewModel( pluginId, CreateInstallActionFor( pluginId ) );
                PluginUpdates.Add( plugin );
            }
            return plugin;
        }

        Action<IDownloadResult> CreateInstallActionFor( INamedVersionedUniqueId pluginId )
        {
            return d => _manualInstaller( pluginId, d );
        }
    }
}
