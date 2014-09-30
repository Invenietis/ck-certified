using System;
using System.Diagnostics;
using System.Globalization;
using CK.Plugin;

namespace CommonServices
{
    public interface IProcessMonitor : IDynamicService
    {
        event EventHandler<ProcessEventArgs> ProcessStarted;

        event EventHandler<ProcessEventArgs> ProcessStopped;

        event EventHandler<ProcessEventArgs> ForegroundProcessChanged;
    }

    public class ProcessEventArgs : EventArgs
    {
        readonly string _processName;
        /// <summary>
        /// Process name (executable name without the extension).
        /// </summary>
        public string ProcessName
        {
            get
            {
                if( _process != null )
                {
                    return Process.ProcessName;
                }
                else
                {
                    return _processName;
                }
            }
        }

        readonly Process _process;
        /// <summary>
        /// A Process item, if applicable.
        /// If this comes from a ProcessStopped event, it will be null.
        /// </summary>
        public Process Process { get { return _process; } }

        public ProcessEventArgs( Process process )
        {
            _process = process;
        }
        public ProcessEventArgs( string processName )
        {
            _processName = TrimProcessName( processName );
        }

        static string TrimProcessName( string s )
        {
            if( s.EndsWith( ".exe", true, CultureInfo.InvariantCulture ) ) return s.Substring( 0, s.Length - 4 );
            if( s.EndsWith( "." ) ) return s.Substring( 0, s.Length - 1 );
            return s;
        }
    }
}
