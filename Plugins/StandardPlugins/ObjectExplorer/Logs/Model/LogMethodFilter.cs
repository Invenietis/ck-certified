using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.StandardPlugins.ObjectExplorer
{
    /// <summary>
    /// Method logging configuration
    /// </summary>
    [Flags]
    public enum LogMethodFilter
    {
        /// <summary>
        /// Nothing is logged.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Caller is logged.
        /// </summary>
        Caller = 1,
        
        /// <summary>
        /// Call parameters are logged.
        /// </summary>
        Parameters = 2,
        
        /// <summary>
        /// Returned value (if any) must be logged.
        /// </summary>
        ReturnValue = 4
    }
}
