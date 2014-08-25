#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\Help\Update\ManualUpdate\PluginUpdateViewModel.cs) is part of CiviKey. 
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

using System.Windows.Input;
using Caliburn.Micro;
using CK.Core;
using Help.Services;
using CK.WPF.ViewModel;

namespace Help.Update.ManualUpdate
{
    public enum PluginUpdateState
    {
        Downloading = 1,
        WaitingForInstall = 2,
        Installing = 3,
        Installed = 4
    }

    public class PluginUpdateViewModel : PropertyChangedBase
    {
        PluginUpdateState _state;
        IDownloadResult _downloadResult;

        public PluginUpdateViewModel( INamedVersionedUniqueId pluginId, System.Action<IDownloadResult> InstallAction )
        {
            PluginId = pluginId;
            InstallCommand = new VMCommand( () =>
            {
                State = PluginUpdateState.Installing;
                InstallAction( DownloadResult );
            },
            ( o ) =>
            {
                return State == PluginUpdateState.WaitingForInstall;
            } );
        }

        public INamedVersionedUniqueId PluginId { get; private set; }

        public PluginUpdateState State
        {
            get { return _state; }
            set
            {
                if( _state != value )
                {
                    _state = value;
                    NotifyOfPropertyChange( () => State );
                }
            }
        }

        public IDownloadResult DownloadResult
        {
            get { return _downloadResult; }
            set
            {
                if( _downloadResult != value )
                {
                    _downloadResult = value;
                    NotifyOfPropertyChange( () => DownloadResult );
                }
            }
        }

        public ICommand InstallCommand { get; private set; }
    }
}
