using System.IO;

namespace Help.Services
{
    /// <summary>
    /// Interface used to mark plugins that have default help contents.
    /// </summary>
    public interface IHaveDefaultHelp
    {
        /// <summary>
        /// Returns the default help content for the plugin. 
        /// It allow another plugin to discover helpable plugins and install the default content
        /// </summary>
        Stream GetDefaultHelp();
    }
}
