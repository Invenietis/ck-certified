using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.StandardPlugins.ObjectExplorer
{
    public interface ILogPropertyConfig
    {
        //Necessary info to identify the property
        string Name { get; }
        
        string PropertyType { get; }

        //Log configuration
        bool DoLogErrors { get; }

        LogPropertyFilter LogFilter { get; }
        
        bool DoLog { get; }        
    } 
}
