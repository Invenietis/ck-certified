using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Plugin;

namespace CommonServices.Accessibility
{
    public interface IHelpService : IDynamicService
    {
        /// <summary>
        /// Gets the help content for the given <see cref="INamedVersionedUniqueId">pluginName</see>.
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <param name="onComplete">delegate invoked when the help will be loaded</param>
        /// <returns>A cancellation token used to cancel the loading if needed</returns>
        CancellationTokenSource GetHelpContentFor( INamedVersionedUniqueId pluginName, Action<Task<string>> onComplete );

        /// <summary>
        /// Open the default web browser to the plugin's help
        /// </summary>
        /// <param name="pluginName">The plugin name in order to find the help</param>
        /// <param name="force">Set force to true to open the web browser even is the content was not found</param>
        /// <returns>True if the content was found, false otherwise</returns>
        bool ShowHelpFor( INamedVersionedUniqueId pluginName, bool force = false );
    }
}
