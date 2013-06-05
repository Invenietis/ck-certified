#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\Logs\Impl\LogConfig.cs) is part of CiviKey. 
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
        CKReadOnlyCollectionOnICollection<ILogServiceConfig> _servicesEx;
        bool _doLog;

        ICKReadOnlyCollection<ILogServiceConfig> ILogConfig.Services { get { return _servicesEx; } }

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
            _servicesEx = new CKReadOnlyCollectionOnICollection<ILogServiceConfig>( _services );
        }
    }
}
