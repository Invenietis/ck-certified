#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\Logs\Impl\ServiceHostConfiguration.cs) is part of CiviKey. 
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

namespace CK.Plugins.ObjectExplorer
{
    public class ServiceHostConfiguration : IServiceHostConfiguration
    {
        ILogConfig _logConfig;

        public ILogConfig LogConfig { get { return _logConfig; } }

        public void ApplyConfiguration( ILogConfig c )
        {
            _logConfig = c.Clone();
        }

        public void ApplyConfiguration( ILogServiceConfig s )
        {
            if( _logConfig != null )
                _logConfig = _logConfig.CombineWith( s );
        }

        public ServiceHostConfiguration( ILogConfig c )
        {
            ApplyConfiguration( c );
        }

        #region IServiceHostConfiguration Members

        public ServiceLogEventOptions GetOptions( System.Reflection.EventInfo e )
        {
            if( !_logConfig.DoLog ) return ServiceLogEventOptions.None;

            foreach( ILogServiceConfig s in _logConfig.Services )
            {
                if( e.DeclaringType.FullName == s.Name)
                {
                    if( !s.DoLog ) return ServiceLogEventOptions.None;

                    foreach( ILogEventConfig lE in s.Events )
                    {
                        if( lE.Name == e.Name )
                        {
                            if( !lE.DoLog ) return ServiceLogEventOptions.None;
                            return lE.LogOptions;
                        }
                    }
                    break;
                }
            }
            return ServiceLogEventOptions.None;
        }

        public ServiceLogMethodOptions GetOptions( System.Reflection.MethodInfo m )
        {
            if( !_logConfig.DoLog ) return ServiceLogMethodOptions.None;

            foreach( ILogServiceConfig s in _logConfig.Services )
            {
                if( m.DeclaringType.FullName == s.Name )
                {
                    if( !s.DoLog ) return ServiceLogMethodOptions.None;

                    foreach( ILogMethodConfig lM in s.Methods )
                    {
                        if( lM.GetSimpleSignature() == m.GetSimpleSignature() )
                        {
                            if( !lM.DoLog ) return ServiceLogMethodOptions.None;
                            return lM.LogOptions;
                        }
                    }
                    break;
                }
            }
            return ServiceLogMethodOptions.None;
        }

        #endregion
    }
}
