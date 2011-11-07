using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Hosting;
using CK.Plugin;

namespace CK.StandardPlugins.ObjectExplorer
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
            foreach( ILogServiceConfig s in _logConfig.Services )
            {
                if( e.DeclaringType.FullName == s.Name )
                {
                    foreach( ILogEventConfig lE in s.Events )
                    {
                        if( lE.Name == e.Name )
                        {
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
            foreach( ILogServiceConfig s in _logConfig.Services )
            {
                if( m.DeclaringType.FullName == s.Name )
                {
                    foreach( ILogMethodConfig lM in s.Methods )
                    {
                        if( lM.GetSimpleSignature() == m.GetSimpleSignature() )
                        {
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
