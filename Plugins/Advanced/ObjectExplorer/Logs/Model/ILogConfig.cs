using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Plugins.ObjectExplorer
{
    public interface ILogConfig
    {
        /// <summary>
        /// Gets the <see cref="IReadOnlyListCollection"/> of <see cref="ILogServiceConfig"/> representing the different running Services
        /// </summary>
        IReadOnlyCollection<ILogServiceConfig> Services { get; }

        /// <summary>
        /// Gets whether Logging is ON or OFF
        /// </summary>
        bool DoLog { get; }
    }
}
