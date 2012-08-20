using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Host.Services
{
    public interface IMinimizable
    {
        void ToggleMinimize();
        bool IsMinimized { get; }
    }
}
