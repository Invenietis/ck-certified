using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Plugins.ObjectExplorer
{
    internal class LogConfig : ILogConfig
    {
        List<ILogServiceConfig> _services;
        ReadOnlyCollectionOnICollection<ILogServiceConfig> _servicesEx;
        bool _doLog;

        IReadOnlyCollection<ILogServiceConfig> ILogConfig.Services { get { return _servicesEx; } }

        public List<ILogServiceConfig> Services
        {
            get { return _services; }
        }
        
        public bool DoLog
        { 
            get { return _doLog; } 
            set { _doLog = value; } 
        }

        public LogConfig()
            : this(new List<ILogServiceConfig>(), false)
        {
        }

        public LogConfig(List<ILogServiceConfig> services, bool doLog)
        {
            _doLog = doLog;
            _services = services;
            _servicesEx = new ReadOnlyCollectionOnICollection<ILogServiceConfig>(_services);
        }
    }
}
