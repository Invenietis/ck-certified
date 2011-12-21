using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Plugins.ObjectExplorer
{
    public interface ILogParameterInfo
    {
        string ParameterName { get; }
        string ParameterType { get; }
    }
}
