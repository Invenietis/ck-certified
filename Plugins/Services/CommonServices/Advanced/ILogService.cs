using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using System.Windows;

namespace CommonServices
{
    public interface ILogService : IDynamicService
    {
        /// <summary>
        /// Event fired when a log as been formatted & sent by the underlying LogPlugin
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
