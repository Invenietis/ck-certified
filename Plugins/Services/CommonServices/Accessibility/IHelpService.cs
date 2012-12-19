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
        CancellationTokenSource GetHelpFor( INamedVersionedUniqueId pluginName, Action<Task<string>> onComplete );
    }
}
