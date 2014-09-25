#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\Logs\Impl\LogPropertyConfig.cs) is part of CiviKey. 
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
