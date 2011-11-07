using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.StandardPlugins.ObjectExplorer
{
    /// <summary>
    /// Property logging configuration
    /// </summary>
    [Flags]
    public enum LogPropertyFilter
    {
        None = 0,
        Caller = 1,
        Get = 2,
        Set = 4
    }
}
