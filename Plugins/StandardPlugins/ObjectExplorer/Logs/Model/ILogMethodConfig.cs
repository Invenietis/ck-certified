using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CK.Core;
using CK.Plugin;

namespace CK.StandardPlugins.ObjectExplorer
{
    public interface ILogMethodConfig
    {
        // Necessary info to identify the method
        string ReturnType { get; }
        string Name { get; }
        IReadOnlyList<ILogParameterInfo> Parameters { get; }

        //Log configuration
        //bool DoLogErrors { get; }
        ServiceLogMethodOptions LogOptions { get; }
        bool DoLog { get; }        
    }
}
