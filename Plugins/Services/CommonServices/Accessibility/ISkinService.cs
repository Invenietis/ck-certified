using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using System.Windows;

namespace CommonServices
{
    public interface ISkinService : IDynamicService
    {
        /// <summary>
        /// Hides the skin window
        /// </summary>
        void Hide();

        /// <summary>
        /// Restores the skin window.
        /// </summary>
        void RestoreSkin();
    }
}
