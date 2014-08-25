#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\Logs\Impl\LogEventConfig.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Collections.Generic;
using CK.Core;
using CK.Plugin;

namespace CK.Plugins.ObjectExplorer
{
    internal class LogEventConfig : ILogEventConfig
    {
        List<ILogParameterInfo> _parameters;
        CKReadOnlyListOnIList<ILogParameterInfo> _parametersEx;
        ICKReadOnlyList<ILogParameterInfo> ILogEventConfig.Parameters { get { return _parametersEx; } }
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
            _parametersEx = new CKReadOnlyListOnIList<ILogParameterInfo>(_parameters);

            LogOptions = logOptions;
        }     
    }    
}
