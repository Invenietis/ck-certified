#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\Help\Update\ManualUpdate\MainViewModel.cs) is part of CiviKey. 
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
