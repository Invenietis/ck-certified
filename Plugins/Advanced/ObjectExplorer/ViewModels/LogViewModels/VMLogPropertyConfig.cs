#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\LogViewModels\VMLogPropertyConfig.cs) is part of CiviKey. 
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

using CK.Plugin;

namespace CK.Plugins.ObjectExplorer.ViewModels.LogViewModels
{
    public class VMLogPropertyConfig : VMLogBaseElement, ILogPropertyConfig
    {
        #region CreateFrom Methods

        public static VMLogPropertyConfig CreateFrom( ISimplePropertyInfo p )
        {
            VMLogPropertyConfig result = new VMLogPropertyConfig( p.Name, true );
            result.PropertyType = p.PropertyType.ToString();
            return result;
        }

        public static VMLogPropertyConfig CreateFrom( ILogPropertyConfig p )
        {
            VMLogPropertyConfig result = new VMLogPropertyConfig( p.Name, false );
            result._doLog = p.DoLog;
            result.PropertyType = p.PropertyType;
            result.LogFilter = p.LogFilter;
            result.DoLogErrors = p.DoLogErrors;
            return result;
        }

        #endregion

        #region Properties

        private bool _doLogCaller;
        private bool _doLogGet;
        private bool _doLogSet;
        private bool _doLogErrors;

        public bool DoLogCaller 
        { 
            get { return _doLogCaller; }
            set { _doLogCaller = value; OnPropertyChanged("DoLogCaller"); OnLogConfigChanged("DoLogCaller"); } 
        }
        public bool DoLogGet 
        { 
            get { return _doLogGet; }
            set { _doLogGet = value; OnPropertyChanged("DoLogGet"); OnLogConfigChanged("DoLogGet"); } 
        }
        public bool DoLogSet 
        { 
            get { return _doLogSet; }
            set { _doLogSet = value; OnPropertyChanged("DoLogSet"); OnLogConfigChanged("DoLogSet"); } 
        }        
        public bool DoLogErrors 
        { 
            get { return _doLogErrors; }
            set { _doLogErrors = value; OnPropertyChanged("DoLogErrors"); OnLogConfigChanged("DoLogErrors"); } 
        }        
        
        public string PropertyType { get; internal set; }
        public bool IsEmpty { get { return !(_doLogCaller | _doLogGet | _doLogSet | _doLog); } }
        public LogPropertyFilter LogFilter
        {
            get { return GetPropertyFilter(); }
            set { ProcessLogPropertyFilter(value); }
        }

        #endregion

        #region Constructors

        public VMLogPropertyConfig(string propertyName, bool isBound)
            : this(propertyName,"", false, 0, isBound)
        {            
        }
        public VMLogPropertyConfig(string propertyName,string propertyType, bool doLogErrors, LogPropertyFilter logFilter, bool isBound)
            : base(propertyName, isBound)
        {
            PropertyType = propertyType;
            DoLogErrors = doLogErrors;
            LogFilter = logFilter;
        }
        #endregion

        #region Methods

        public bool UpdateFrom(ILogPropertyConfig p)
        {
            bool hasChanged = false;

            if( p.LogFilter != LogFilter )
            {
                ProcessLogPropertyFilter(p.LogFilter);
                hasChanged = true;
            }

            if (p.DoLogErrors != _doLogErrors)
            {
                DoLogErrors = p.DoLogErrors;
                hasChanged = true;
            }

            if (p.DoLog != DoLog)
            {
                DoLog = p.DoLog;
                hasChanged = true;
            }

            return hasChanged;
        }

        public void ClearConfig()
        {
            DoLog = false;
            DoLogCaller = false;
            DoLogGet = false;
            DoLogSet = false;
            DoLogErrors = false;
        }       

        private LogPropertyFilter GetPropertyFilter()
        {
            LogPropertyFilter l = new LogPropertyFilter();

            if (_doLogCaller) l = l | LogPropertyFilter.Caller;
            if (_doLogGet) l = l | LogPropertyFilter.Get;
            if (_doLogSet) l = l | LogPropertyFilter.Set;

            return l;
        }

        private void ProcessLogPropertyFilter(LogPropertyFilter logPropertyFilter)
        {
            DoLogCaller = ((logPropertyFilter & LogPropertyFilter.Caller) == LogPropertyFilter.Caller);
            DoLogGet = ((logPropertyFilter & LogPropertyFilter.Get) == LogPropertyFilter.Get);
            DoLogSet = ((logPropertyFilter & LogPropertyFilter.Set) == LogPropertyFilter.Set);
        }

        #endregion
    }
}
