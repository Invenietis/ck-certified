#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\CommonServices\Advanced\ILogService.cs) is part of CiviKey. 
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

using System;
using CK.Plugin;

namespace CommonServices
{
    public interface ILogService : IDynamicService
    {
        /// <summary>
        /// Event fired when a log as been formatted and sent by the underlying LogPlugin
        /// </summary>
        event LogTriggeredEventHandler LogTriggered;
    }

    public delegate void LogTriggeredEventHandler( object sender, LogTriggeredEventArgs e );
    public class LogTriggeredEventArgs : EventArgs
    {
        LogEventArgs _logEventArgs;
        public LogEventArgs LogEventArgs { get { return _logEventArgs; } }

        string _logLevel;
        public string LogLevel { get { return _logLevel; } }

        string _content;
        public string Content { get { return _content; } }

        public LogTriggeredEventArgs( LogEventArgs e, string content, string logLevel )
        {
            _logEventArgs = e;    
            _content = content;
            _logLevel = logLevel;
        }

        public LogTriggeredEventArgs( string content, string logLevel )
            : this(null, content, logLevel)
        {
        }
    }
}
