using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Hosting;
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
