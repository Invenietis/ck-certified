using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.StandardPlugins.ObjectExplorer
{
    /// <summary>
    /// Event logging configuration
    /// </summary>
    [Flags]
    public enum LogEventFilter
    {
        None = 0,
        Caller = 1,
        Parameters = 2,
        Delegates = 4
    }
}
