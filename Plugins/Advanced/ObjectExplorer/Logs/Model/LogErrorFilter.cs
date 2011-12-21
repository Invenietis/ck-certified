using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Plugins.ObjectExplorer
{
    /// <summary>
    /// Enum representing Error logging configuration
    /// Protecting an error means catching the exception that is thrown by a subscriber.
    /// </summary>
    [Flags]
    public enum LogEventErrorFilter
    {
        None = 0,
        Log = 1,
        Protect = 2
    }
}
