using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using System.Reflection;
using CK.Core;

namespace CK.Plugins.ObjectExplorer
{
    public static class LogConfigExtensions
    {
        /// <summary>
        /// Creates a snapshot of the <see cref="ILogConfig"/>.
        /// </summary>
        /// <param name="config"><see cref="ILogConfig"/> to clone.</param>
        /// <returns>A clone of the configuration object.</returns>
        public static ILogConfig Clone( this ILogConfig config )
        {
            LogConfig c = new LogConfig();
            c.DoLog = config.DoLog;
            foreach( ILogServiceConfig s in config.Services )
            {
                c.Services.Add( Clone( s ) );
            }
            return c;
        }

        /// <summary>
        /// Returns a <see cref="ILogServiceConfig"/> snapshot of the <see cref="ILogServiceConfig"/> set as parameter
        /// </summary>
        /// <param name="source"><see cref="ILogServiceConfig"/> to copy</param>
        /// <returns></returns>
        private static ILogServiceConfig Clone( ILogServiceConfig source )
        {
            LogEventConfig e;
            LogMethodConfig m;
            LogServiceConfig s;

            s = new LogServiceConfig( source.Name );

            s.DoLog = source.DoLog;

            foreach( ILogEventConfig eToCopy in source.Events )
            {
                e = new LogEventConfig( eToCopy.Name, new List<ILogParameterInfo>(), eToCopy.LogOptions, eToCopy.DoLog );
                foreach( ILogParameterInfo logParameterInfoToCopy in eToCopy.Parameters )
                    e.Parameters.Add( new LogParameterInfo( logParameterInfoToCopy.ParameterName, logParameterInfoToCopy.ParameterType ) );
                s.Events.Add( e );
            }

            foreach( ILogMethodConfig mToCopy in source.Methods )
            {
                m = new LogMethodConfig( mToCopy.Name, mToCopy.ReturnType, new List<ILogParameterInfo>(), mToCopy.LogOptions, mToCopy.DoLog );
                foreach( ILogParameterInfo parameterToCopy in mToCopy.Parameters )
                    m.Parameters.Add( new LogParameterInfo( parameterToCopy.ParameterName, parameterToCopy.ParameterType ) );
                s.Methods.Add( m );
            }

            foreach( ILogPropertyConfig pToCopy in source.Properties )
                s.Properties.Add( new LogPropertyConfig( pToCopy.PropertyType, pToCopy.Name, pToCopy.DoLogErrors, pToCopy.LogFilter, pToCopy.DoLog ) );

            return s;
        }

        /// <summary>
        /// Creates a new <see cref="ILogConfig"/> that is a clone of this configuration with a clone of the <see cref="ILogServiceConfig"/> parameter.
        /// </summary>
        /// <param name="config">This <see cref="ILogConfig"/>.</param>
        /// <param name="s">A <see cref="ILogServiceConfig"/> which log configuration needs to be applied.</param>
        /// <returns>A clone of this configuration with a clone of the <paramref name="s"/> service config.</returns>
        public static ILogConfig CombineWith( this ILogConfig config, ILogServiceConfig s )
        {
            LogConfig c = (LogConfig)config.Clone();
            for( int i = 0; i < c.Services.Count; i++ )
            {
                if( c.Services[i].Name == s.Name )
                {
                    c.Services.RemoveAt( i );
                    break;
                }
            }
            c.Services.Add( Clone( s ) );
            return c;
        }
    }
}
