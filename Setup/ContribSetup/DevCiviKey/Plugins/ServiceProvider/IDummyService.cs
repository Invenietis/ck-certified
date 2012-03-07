using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace DummyPlugins
{
    /// <summary>
    /// A service MUST implement IDynamicService, this interface has no methods nor properties, it is used as a flag that descrbies this interface as a service
    /// </summary>
    public interface IDummyService : IDynamicService
    {
        int GetAnswerToLife();
    }
}
