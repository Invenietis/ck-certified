#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\Logs\Impl\LogServiceConfig.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Collections.Generic;
using CK.Core;

namespace CK.Plugins.ObjectExplorer
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
