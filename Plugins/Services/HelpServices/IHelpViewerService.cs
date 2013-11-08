using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Plugin;

namespace Help.Services
{
    public interface IHelpViewerService : IDynamicService
    {
        /// <summary>
        /// Open the default web browser to the plugin's help
        /// </summary>
        /// <param name="pluginName">The plugin name in order to find the help</param>
        /// <param name="force">Set force to true to open the web browser even is the content was not found</param>
        /// <returns>True if the content was found, false otherwise</returns>
        bool ShowHelpFor( IVersionedUniqueId pluginName, bool force = false );
    }
}
