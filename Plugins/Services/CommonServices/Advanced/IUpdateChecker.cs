using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Core;

namespace CommonServices
{
    public enum UpdateVersionState
    {
        Unknown,
        CheckingForNewVersion,
        NoNewerVersion,
        NewerVersionAvailable,
        ErrorWhileCheckingVersion
    }

    public enum UpdateDownloadState
    {
        None,
        Downloading,
        Downloaded,
        ErrorWhileDownloading
    }

    public interface IUpdateChecker : IDynamicService
    {
        UpdateVersionState VersionState { get; }
        
        UpdateDownloadState DownloadState { get; }

        event EventHandler StateChanged;

        Version NewVersion { get; }

        bool IsBusy { get; }

        void CheckForUpdate();
        
        void StartDownload();

    }
}
