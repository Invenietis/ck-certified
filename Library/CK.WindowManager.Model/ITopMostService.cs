using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CK.Plugin;

namespace CK.WindowManager.Model
{
    /// <summary>
    /// Creates a system of "z-index" relative to multiple windows in Topmost
    /// </summary>
    public interface ITopMostService : IDynamicService
    {
        /// <summary>
        /// Registers a window in the display hierarchy.
        /// </summary>
        /// <param name="levelName">Index of the Window</param>
        /// <param name="window">Window to register</param>
        /// <returns>Return false, if the window already exist or if int.TryParse fail, otherwise true</returns>
        bool RegisterTopMostElement( string levelName, Window window );

        /// <summary>
        /// Unregister a window in the display hierachy.
        /// </summary>
        /// <param name="window">Window to unregister</param>
        /// <returns>Return false, if the window isn't register</returns>
        bool UnregisterTopMostElement( Window window );

    }
}
