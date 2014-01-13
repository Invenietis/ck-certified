#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\Logs\Impl\LogMethodConfig.cs) is part of CiviKey. 
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
using CK.Plugin;

namespace CK.Plugins.ObjectExplorer
{
    internal class LogMethodConfig : ILogMethodConfig
    {
        List<ILogParameterInfo> _parameters;
        CKReadOnlyListOnIList<ILogParameterInfo> _parametersEx;
        ICKReadOnlyList<ILogParameterInfo> ILogMethodConfig.Parameters { get { return _parametersEx; } }
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
            _parametersEx = new CKReadOnlyListOnIList<ILogParameterInfo>( _parameters );
        }
    }
}
