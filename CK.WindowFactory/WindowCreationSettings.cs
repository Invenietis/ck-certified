using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.WindowFactory
{
    public class WindowCreationSettings
    {
        public bool IsNoFocusWindow { get; set; }
        public bool UseWindowManager { get; set; }
        public bool UseWindowBinder { get; set; }
        public bool UseHighlighter { get; set; }
        public bool UseTopMostService { get; set; }
        public string TopMostLevel { get; set; }

        public object DataContext { get; set; }
        public bool ShowWindow { get; set; }
        public string WindowName { get; set; }
    }
}
