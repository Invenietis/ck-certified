using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CK.Core;
using CK.Plugin;

namespace CK.Plugins.ObjectExplorer
{
    /// <summary>
    /// Represents the log configuration of an event
    /// </summary>
    public interface ILogEventConfig
    {
        //Necessary info to identify the event
        string Name { get; }
        IReadOnlyList<ILogParameterInfo> Parameters { get; }

        //Log configuration
        ServiceLogEventOptions LogOptions { get; }
        //LogEventErrorFilter ErrorFilter { get; }
        //LogEventFilter LogFilter { get; }
        bool DoLog { get; }
    }    
}
