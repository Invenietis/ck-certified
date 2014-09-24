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
        public string ProcessName { get { return _processName; } }

        public ProcessEventArgs( string processName )
        {
            _processName = TrimProcessName( processName );
        }
        public ProcessEventArgs( Process process )
        {
            _processName = process.ProcessName;
        }

        static string TrimProcessName( string s )
        {
            if( s.EndsWith( ".exe", true, CultureInfo.InvariantCulture ) ) return s.Substring( 0, s.Length - 4 );
            if( s.EndsWith( "." ) ) return s.Substring( 0, s.Length - 1 );
            return s;
        }
    }
}
