    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Plugins.ObjectExplorer
{
    public interface ILogServiceConfig
    {
        /// <summary>
        /// Full name of the service (type name with its namespace).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets a boolean that enables/disables any log for this service.
        /// </summary>
        bool DoLog { get; }


        IReadOnlyCollection<ILogMethodConfig> Methods { get; }

        IReadOnlyCollection<ILogEventConfig> Events { get; }
        
        IReadOnlyCollection<ILogPropertyConfig> Properties { get; }
      
    }
}
