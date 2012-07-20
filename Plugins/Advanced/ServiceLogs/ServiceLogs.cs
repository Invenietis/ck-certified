using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Context;
using Host.Services;
using CK.Plugin.Hosting;
using CK.Core;
using System.Collections;
using System.IO;
using System.Diagnostics;
using CommonServices;
using ServiceLogs;
using CK.Windows.App;

namespace LogPlugin
{
    /// <summary>
    /// Plugin that creates logs for developpers (CiviKey's life cycle & errors).
    /// </summary>
    [Plugin( PluginIdString, PublicName = PluginPublicName, Version = PluginIdVersion, Categories = new string[] { "Advanced" } )]
    public class ServiceLogs : IPlugin, ILogService
    {
        const string PluginIdString = "{FEA8570C-2ECE-44b3-B1CE-0DBA414D5045}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "ServiceLogs";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        static Common.Logging.ILog _log = Common.Logging.LogManager.GetLogger<ServiceLogs>();

        EventHandler<LogEventArgs> _hCreating;
        EventHandler<LogEventArgs> _hCreated;
        StringWriter _buffer;

        [RequiredService]
        public IContext Context { get; set; }

        [RequiredService( Required = false )]
        public INotificationService Notification { get; set; }

        public ServiceLogs()
        {
            _hCreated = new EventHandler<LogEventArgs>( OnEventCreated );
            _hCreating = new EventHandler<LogEventArgs>( OnEventCreating );
        }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            Context.LogCenter.EventCreating += _hCreating;
            Context.LogCenter.EventCreated += _hCreated;
            _buffer = new StringWriter();
            foreach( ILogErrorCaught error in Context.LogCenter.UntrackedErrors )
            {
                LogHostEventArgs e = error as LogHostEventArgs;
                if( e != null )
                {
                    if( e.IsCreating ) OnEventCreating( this, e );
                    else OnEventCreated( this, e );
                }
            }
        }

        public void Stop()
        {
            Context.LogCenter.EventCreating -= _hCreating;
            Context.LogCenter.EventCreated -= _hCreated;
            _buffer.Dispose();
            _buffer = null;
        }

        public void Teardown()
        {
        }

        #endregion

        void OnEventCreating( object sender, LogEventArgs e )
        {
            _buffer.GetStringBuilder().Length = 0;
            Process( _buffer, e );
            Log( e, _buffer.GetStringBuilder().ToString(), Common.Logging.LogLevel.Info );
        }

        void OnEventCreated( object sender, LogEventArgs e )
        {
            _buffer.GetStringBuilder().Length = 0;
            NotifyError er = Process( _buffer, e );
            string message = _buffer.GetStringBuilder().ToString();
            if( er != null )
            {
                Log( e, message, Common.Logging.LogLevel.Error );
                bool errorLogCreated;
                using( var fileLog = CrashLogManager.CreateNew( "errorLog" ) )
                {
                    errorLogCreated = fileLog.IsValid;
                    fileLog.Writer.Write( message );
                }
                if( Notification != null )
                {
                    if( !errorLogCreated )
                    {
                        Notification.ShowNotification( PluginId.UniqueId, R.CriticalError, R.ErrorOccuredWhileLogging, 4000, NotificationTypes.Error );
                    }
                    else
                    {
                        Notification.ShowNotification( PluginId.UniqueId, er.Title, er.Content, 2000, NotificationTypes.Error );
                        PlanLogUpload();
                    }
                }
            }
            else Log( e, message, Common.Logging.LogLevel.Info );
        }

        /// <summary>
        /// Returns true if the event is an error (holds an exception).
        /// </summary>
        static NotifyError Process( TextWriter w, LogEventArgs e )
        {
            if( e.IsCreating )
            {
                w.NewLine = Environment.NewLine + new String( ' ', e.Depth );
                CrashLogWriter.WriteLineProperty( w, "Entering", e.GetType().Name );
                WriteLineMember( w, e );
                WriteLineParameters( w, e );
                WriteLineReturnValueAndCaller( w, e );
                return null;
            }
            else
            {
                CrashLogWriter.WriteLineProperty( w, "Log", e.GetType().Name );
                WriteLineMember( w, e );
                WriteLineParameters( w, e );
                WriteLineReturnValueAndCaller( w, e );
                NotifyError n = WriteLineError( w, e );
                w.NewLine = Environment.NewLine + new String( ' ', e.Depth );
                return n;
            }
        }

        private static void WriteLineReturnValueAndCaller( TextWriter w, LogEventArgs e )
        {
            var m = e as ILogMethodEntry;
            if( m != null )
            {
                if( m.ReturnValue != null ) CrashLogWriter.WriteLineProperties( w, "Return Value", new object[] { m.ReturnValue.ToString() } );
                if( m.Caller != null ) CrashLogWriter.WriteLineProperties( w, "Caller", new object[] { m.Caller.ToString() } );
            }
        }

        private static void WriteLineParameters( TextWriter w, LogEventArgs e )
        {
            var p = e as ILogWithParametersEntry;
            if( p != null )
            {
                if( p.Parameters != null && p.Parameters.Length > 0 ) CrashLogWriter.WriteLineProperties( w, "Parameters", p.Parameters );
            }
        }

        private static void WriteLineMember( TextWriter w, LogEventArgs e )
        {
            var m = e as ILogInterceptionEntry;
            if( m != null )
            {
                CrashLogWriter.WriteLineProperty( w, "Member", m.Member.DeclaringType.Name + '.' + m.Member.Name );
            }
        }

        class NotifyError
        {
            public string Title;
            public string Content;
        }

        private static NotifyError WriteLineError( TextWriter w, LogEventArgs e )
        {
            Debug.Assert( typeof( ILogErrorCulprit ).IsAssignableFrom( typeof( ILogEventNotRunningError ) )
                            && typeof( ILogErrorCulprit ).IsAssignableFrom( typeof( ILogErrorCaught ) ), "These 2 interfaces both are ILogErrorCulprit." );
            Debug.Assert( typeof( ILogErrorCaught ).IsAssignableFrom( typeof( ILogEventNotRunningError ) ) == false
                            && typeof( ILogEventNotRunningError ).IsAssignableFrom( typeof( ILogErrorCaught ) ) == false, "These 2 interfaces are independant." );

            var culprit = e as ILogErrorCulprit;
            if( culprit != null )
            {
                NotifyError notif = new NotifyError();
                notif.Title = String.Format( R.NotifiyErrorTitle, culprit.Culprit.DeclaringType.Name );

                CrashLogWriter.WriteLineProperty( w, "Culprit", culprit.Culprit.DeclaringType.FullName + '.' + culprit.Culprit.Name );
                var error = e as ILogErrorCaught;
                if( error != null )
                {
                    CrashLogWriter.WriteLineException( w, error.Error );
                    notif.Content = error.Error.Message;
                }
                else
                {
                    var runningError = e as ILogEventNotRunningError;
                    if( runningError != null )
                    {
                        CrashLogWriter.WriteLineProperty( w, "ServiceStatus", runningError.ServiceIsDisabled ? "Disabled" : "Stopped" );
                        notif.Content = R.ErrorEventServiceStatus;
                    }
                }
                return notif;
            }
            return null;
        }

        void PlanLogUpload()
        {
        }


        public event LogTriggeredEventHandler LogTriggered;

        private void Log( string message, Common.Logging.LogLevel logLevel )
        {
            Log( null, message, logLevel );
        }

        private void Log( LogEventArgs e, string message, Common.Logging.LogLevel logLevel )
        {
            switch( logLevel )
            {
                case Common.Logging.LogLevel.Debug:
                    _log.Debug( message );
                    break;
                case Common.Logging.LogLevel.Error:
                    _log.Error( message );
                    break;
                case Common.Logging.LogLevel.Fatal:
                    _log.Fatal( message );
                    break;
                case Common.Logging.LogLevel.Info:
                case Common.Logging.LogLevel.Trace:
                    _log.Info( message );
                    break;
                case Common.Logging.LogLevel.Warn:
                    _log.Warn( message );
                    break;
                default:
                    _log.Info( message );
                    break;
            }

            //Enable other plugins to get formatted logs
            if( LogTriggered != null )
            {
                LogTriggered( this, new LogTriggeredEventArgs( e, message, logLevel.ToString() ) );
            }
        }
    }
}
