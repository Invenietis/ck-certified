using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Plugin;

namespace Help.Services
{
    public interface IHelpUpdaterService : IDynamicService
    {
        /// <summary>
        /// Fired when a new update is available for a plugin.
        /// At this point the update content is not dowloaded yet.
        /// </summary>
        event EventHandler<HelpUpdateEventArgs> UpdateAvailable;

        /// <summary>
        /// Fired when a new update content is available on the file system.
        /// At this point the update can be inspected and installed.
        /// </summary>
        event EventHandler<HelpUpdateDownloadedEventArgs> UpdateDownloaded;

        /// <summary>
        /// Fired when a new update content is installed.
        /// </summary>
        event EventHandler<HelpUpdateDownloadedEventArgs> UpdateInstalled;

        void StartManualUpdate();
    }

    public class HelpUpdateEventArgs : EventArgs
    {
        public HelpUpdateEventArgs( INamedVersionedUniqueId plugin )
        {
            Plugin = plugin;
        }

        public INamedVersionedUniqueId Plugin { get; private set; }
    }

    public class HelpUpdateDownloadedEventArgs : HelpUpdateEventArgs
    {
        public HelpUpdateDownloadedEventArgs( INamedVersionedUniqueId plugin, IDownloadResult downloadResult )
            : base( plugin )
        {
            DownloadResult = downloadResult;
        }

        public IDownloadResult DownloadResult { get; private set; }
    }
}
