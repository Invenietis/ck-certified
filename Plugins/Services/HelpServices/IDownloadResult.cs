using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace Help.Services
{
    public interface IDownloadResult
    {
        string Culture { get; }
        TemporaryFile File { get; }
        string Version { get; }
    }
}
