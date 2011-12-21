using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CK.Core;
using CK.Plugin;

namespace CK.Plugins.ObjectExplorer
{
    internal class LogMethodConfig : ILogMethodConfig
    {
        List<ILogParameterInfo> _parameters;
        ReadOnlyListOnIList<ILogParameterInfo> _parametersEx;
        IReadOnlyList<ILogParameterInfo> ILogMethodConfig.Parameters { get { return _parametersEx; } }
        bool _doLog;
        //bool _doLogErrors;

        public string Name { get; set; }
        public string ReturnType { get; set; }
        public bool DoLog { get { return _doLog; } set { _doLog = value; } }
        public List<ILogParameterInfo> Parameters { get { return _parameters; } }
        //public bool DoLogErrors { get { return _doLogErrors; } set { _doLogErrors = value; } }
        public ServiceLogMethodOptions LogOptions { get; set; }

        public LogMethodConfig()
            : this( "", "", new List<ILogParameterInfo>(), 0, false )
        {
        }

        public LogMethodConfig( string methodName, string returnType, List<ILogParameterInfo> p, ServiceLogMethodOptions logOptions, bool doLog )
        {
            _doLog = doLog;            
            LogOptions = logOptions;
            Name = methodName;
            ReturnType = returnType;
            _parameters = p;
            _parametersEx = new ReadOnlyListOnIList<ILogParameterInfo>( _parameters );
        }
    }
}
