using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
