using System.Collections.Generic;
using CK.Core;
using CK.Plugin;

namespace CK.Plugins.ObjectExplorer
{
    internal class LogEventConfig : ILogEventConfig
    {
        List<ILogParameterInfo> _parameters;
        ReadOnlyListOnIList<ILogParameterInfo> _parametersEx;
        IReadOnlyList<ILogParameterInfo> ILogEventConfig.Parameters { get { return _parametersEx; } }
        bool _doLog;

        public string Name { get; internal set; }
        //public LogEventErrorFilter ErrorFilter { get; set; }
        //public LogEventFilter LogFilter { get; set; }

        public ServiceLogEventOptions LogOptions { get; set; }

        public bool DoLog { get { return _doLog; } set { _doLog = value; } }        
        public List<ILogParameterInfo> Parameters { get { return _parameters; } }

        public LogEventConfig()
            : this("",new List<ILogParameterInfo>(),0,false)
        {
        }

        public LogEventConfig(string eventName, List<ILogParameterInfo> parameters, ServiceLogEventOptions logOptions, bool doLog)
        {
            Name = eventName;
            _doLog = doLog;   
            _parameters = parameters;
            _parametersEx = new ReadOnlyListOnIList<ILogParameterInfo>(_parameters);

            LogOptions = logOptions;
        }     
    }    
}
