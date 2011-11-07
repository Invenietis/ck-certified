using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.StandardPlugins.ObjectExplorer
{
    public class LogParameterInfo : ILogParameterInfo
    {
        public string ParameterName { get; set; }
        public string ParameterType { get; set; }

        public LogParameterInfo()
            : this("","")
        {

        }

        public LogParameterInfo(string parameterName, string parameterType)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
        }
    }
}
