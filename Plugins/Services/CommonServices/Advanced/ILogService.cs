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
        DateTime _triggerTime;
        public DateTime TriggerTime { get { return _triggerTime; } }

        string _logLevel;
        public string LogLevel { get { return _logLevel; } }

        string _content;
        public string Content { get { return _content; } }

        public LogTriggeredEventArgs( string content, string logLevel )
        {
            _content = content;
            _logLevel = logLevel;
        }
    }
}
