using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CommonServices
{
    public interface IFileLauncherCommandHandlerService : IDynamicService
    {
        void LaunchFile(string command);
    }
}
