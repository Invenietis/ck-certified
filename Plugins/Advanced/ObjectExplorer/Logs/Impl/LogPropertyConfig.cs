using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Plugins.ObjectExplorer
{
    internal class LogPropertyConfig : ILogPropertyConfig
    {
        bool _doLog;
        bool _doLogErrors;

        public string Name { get; set; }
        public string PropertyType { get; set; }       
        public bool DoLog { get { return _doLog; } set { _doLog = value; } }
        public bool DoLogErrors { get { return _doLogErrors; } set { _doLogErrors = value; } }
        public LogPropertyFilter LogFilter { get; set; }

        public LogPropertyConfig()
            : this("","", false, 0, false)
        {
        }

        public LogPropertyConfig(string propertyType, string propertyName,bool logErrors, LogPropertyFilter logFilter, bool doLog)
        {
            Name = propertyName;
            _doLog = doLog;
            PropertyType = propertyType;
            _doLogErrors = logErrors;
            LogFilter = logFilter;
            _doLog = doLog;
        }

        
    } 
}
