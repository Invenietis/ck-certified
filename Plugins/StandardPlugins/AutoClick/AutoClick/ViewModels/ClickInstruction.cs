using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.StandardPlugins.AutoClick.Model
{
    public enum ClickInstruction
    {
        None = 0,
        RightButtonDown = 1,
        RightButtonUp = 2,
        LeftButtonDown = 3,
        LeftButtonUp = 4,
        WheelDown = 5,
        WheelUp = 6
    }
}
