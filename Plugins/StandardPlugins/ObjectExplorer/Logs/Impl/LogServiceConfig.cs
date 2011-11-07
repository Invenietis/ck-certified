using System.Collections.Generic;
using CK.Core;

namespace CK.StandardPlugins.ObjectExplorer
{
    internal class LogServiceConfig : ILogServiceConfig
    {

        List<ILogMethodConfig> _methods;
        ReadOnlyCollectionOnICollection<ILogMethodConfig> _methodsEx;
        IReadOnlyCollection<ILogMethodConfig> ILogServiceConfig.Methods { get { return _methodsEx; } }

        List<ILogEventConfig> _events;
        ReadOnlyCollectionOnICollection<ILogEventConfig> _eventsEx;
        IReadOnlyCollection<ILogEventConfig> ILogServiceConfig.Events { get { return _eventsEx; } }

        List<ILogPropertyConfig> _properties;
        ReadOnlyCollectionOnICollection<ILogPropertyConfig> _propertiesEx;
        IReadOnlyCollection<ILogPropertyConfig> ILogServiceConfig.Properties { get { return _propertiesEx; } }

        bool _doLog;
        string _name;

        public List<ILogEventConfig> Events { get { return _events; } }
        public List<ILogPropertyConfig> Properties { get { return _properties; } }
        public List<ILogMethodConfig> Methods { get { return _methods; } }
        public bool DoLog { get { return _doLog; } set { _doLog = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        


        public LogServiceConfig()
            : this("")
        {
        }

        public LogServiceConfig(string name)
        {
            Name = name;

            _methods = new List<ILogMethodConfig>();
            _methodsEx = new ReadOnlyCollectionOnICollection<ILogMethodConfig>(_methods);

            _events = new List<ILogEventConfig>();
            _eventsEx = new ReadOnlyCollectionOnICollection<ILogEventConfig>(_events);

            _properties = new List<ILogPropertyConfig>();
            _propertiesEx = new ReadOnlyCollectionOnICollection<ILogPropertyConfig>(_properties);
        }        
    }
}
