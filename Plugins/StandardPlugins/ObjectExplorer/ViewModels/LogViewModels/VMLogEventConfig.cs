using System.Collections.Generic;
using System.Reflection;
using CK.Plugin.Hosting;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;

namespace CK.StandardPlugins.ObjectExplorer.ViewModels.LogViewModels
{
    public class VMLogEventConfig : VMLogBaseElement, ILogEventConfig
    {

        #region CreateFrom Methods

        public static VMLogEventConfig CreateFrom( VMLogServiceConfig holder, ISimpleEventInfo e )
        {
            VMLogEventConfig result = new VMLogEventConfig(holder, e.Name, 0, true);
            result._holder = holder;
            result._dataPath = result._holder.Name + "_" + result.Name;

            //foreach( ISimpleParameterInfo p in e.Parameters )
            //{
            //    result._parameters.Add( new LogParameterInfo( p.ParameterName, p.ParameterType ) );
            //}

            //If there is no config, we set the default one.
            if( result._holder.Config.User[result._dataPath + "_logoptions"] == null ) result._doLogErrors = true;
            else result.LogOptions = (ServiceLogEventOptions)result._holder.Config.User[result._dataPath + "_logoptions"];  

            if( result._holder.Config.User[result._dataPath + "_globaldolog"] == null ) result._doLog = true;
            else result._doLog = (bool)result._holder.Config.User[result._dataPath + "_globaldolog"];

            return result;
        }

        public static VMLogEventConfig CreateFrom( VMLogServiceConfig holder, ILogEventConfig e )
        {
            VMLogEventConfig result = new VMLogEventConfig( holder, e.Name, e.LogOptions, false );
            result._doLog = e.DoLog;
            foreach( ILogParameterInfo p in e.Parameters )
                result._parameters.Add( new LogParameterInfo( p.ParameterName, p.ParameterType ) );
            return result;
        }

        #endregion

        #region Properties & Variables

        string _dataPath;
        VMLogServiceConfig _holder;
        List<ILogParameterInfo> _parameters;
        IReadOnlyList<ILogParameterInfo> _parametersEx;
        bool _doLogErrors;
        bool _doLogStartRaise;
        bool _doLogParameters;
        bool _doLogEndRaise;
        bool _doCatchEventWhenServiceStopped; //If the service launches the event when it is stopped, catch the error not to crash the application.
        bool _doLogCaughtEventWhenServiceStopped; //If the service launches the event when it is stopped and _doCatchEventWhenServiceStopped is set to true, log the error.
        bool _doCatchBadEventHandling; //If a subscriber raises an exception while handling the event, catch the error not to crash the application. 

        public VMLogServiceConfig Holder { get { return _holder; } }
        public IPluginConfigAccessor Config { get; set; }
        public bool DoLogErrors
        {
            get { return _doLogErrors; }
            set { _doLogErrors = value; OnPropertyChanged( "DoLogErrors" ); OnLogConfigChanged( "DoLogErrors" ); }
        }
        public bool DoLogStartRaise
        {
            get { return _doLogStartRaise; }
            set { _doLogStartRaise = value; OnPropertyChanged( "DoLogStartRaise" ); OnLogConfigChanged( "DoLogStartRaise" ); }
        }
        public bool DoLogParameters 
        { 
            get { return _doLogParameters; }
            set { _doLogParameters = value; OnPropertyChanged("DoLogParameters"); OnLogConfigChanged("DoLogParameters"); } 
        }
        public bool DoLogEndRaise
        {
            get { return _doLogEndRaise; }
            set { _doLogEndRaise = value; OnPropertyChanged( "DoLogEndRaise" ); OnLogConfigChanged( "DoLogEndRaise" ); }
        }
        public bool DoCatchEventWhenServiceStopped
        {
            get { return _doCatchEventWhenServiceStopped; }
            set { _doCatchEventWhenServiceStopped = value; OnPropertyChanged( "DoCatchEventWhenServiceStopped" ); OnLogConfigChanged( "DoCatchEventWhenServiceStopped" ); }
        }
        //BOOKMARK : Enable modification only if DoCatchEventWhenServiceStopped == true ?
        public bool DoLogCaughtEventWhenServiceStopped
        {
            get { return _doLogCaughtEventWhenServiceStopped; }
            set { _doLogCaughtEventWhenServiceStopped = value; OnPropertyChanged( "DoLogCaughtEventWhenServiceStopped" ); OnLogConfigChanged( "DoLogCaughtEventWhenServiceStopped" ); }
        }
        public bool DoCatchBadEventHandling
        {
            get { return _doCatchBadEventHandling; }
            set { _doCatchBadEventHandling = value; OnPropertyChanged( "DoCatchBadEventHandling" ); OnLogConfigChanged( "DoCatchBadEventHandling" ); }
        }

        public bool IsEmpty { get { return !( _doLog | _doLogStartRaise | _doLogParameters | DoLogEndRaise | _doCatchEventWhenServiceStopped | _doLogCaughtEventWhenServiceStopped | _doCatchBadEventHandling ); } }

        public ServiceLogEventOptions LogOptions
        {
            get { return GetLogOptions(); }
            private set { ProcessLogOptions( value ); }
        }

        public List<ILogParameterInfo> Parameters { get { return _parameters; } }
        Core.IReadOnlyList<ILogParameterInfo> ILogEventConfig.Parameters
        {
            get { return _parametersEx; }
        }
        
        #endregion

        #region Constructors

        public VMLogEventConfig(VMLogServiceConfig holder, string name, ServiceLogEventOptions logOptions, bool isBound )
            : this( holder, name, new List<ILogParameterInfo>(), logOptions, isBound )
        {
        }

        public VMLogEventConfig(VMLogServiceConfig holder, string name, List<ILogParameterInfo> parameters, ServiceLogEventOptions logOptions, bool isBound )
            : base( name, isBound )
        {
            _holder = holder;                 
            
            _parameters = parameters;
            _parametersEx = new ReadOnlyListTypeAdapter<ILogParameterInfo, ILogParameterInfo>( _parameters );

            _doLogErrors = ((logOptions & ServiceLogEventOptions.LogErrors) == ServiceLogEventOptions.LogErrors);
            _doLogStartRaise = ((logOptions & ServiceLogEventOptions.StartRaise) == ServiceLogEventOptions.StartRaise);
            _doLogParameters = ((logOptions & ServiceLogEventOptions.LogParameters) == ServiceLogEventOptions.LogParameters);
            _doLogEndRaise = ((logOptions & ServiceLogEventOptions.EndRaise) == ServiceLogEventOptions.EndRaise);
            _doCatchEventWhenServiceStopped = ((logOptions & ServiceLogEventOptions.SilentEventRunningStatusError) == ServiceLogEventOptions.SilentEventRunningStatusError);
            _doLogCaughtEventWhenServiceStopped = ((logOptions & ServiceLogEventOptions.LogSilentEventRunningStatusError) == ServiceLogEventOptions.LogSilentEventRunningStatusError);
            _doCatchBadEventHandling = ((logOptions & ServiceLogEventOptions.SilentEventError) == ServiceLogEventOptions.SilentEventError);

        }
        #endregion

        #region Methods

        internal void UpdatePropertyBag()
        {
            _holder.Config.User[_dataPath + "_logoptions"] = GetLogOptions();
            _holder.Config.User[_dataPath + "_globaldolog"] = DoLog;
        }

        public bool UpdateFrom(ILogEventConfig l)
        {
            bool hasChanged = false;

            if (l.LogOptions != LogOptions)
            {
                ProcessLogOptions( l.LogOptions );
                hasChanged = true;
            }

            if (l.DoLog != DoLog)
            {
                DoLog = l.DoLog;
                hasChanged = true;
            }

            return hasChanged;
        }

        public void UpdateFrom( IPluginConfigAccessor config )
        {
            string path = this.Name + "_logoptions";            
            if( config.User[path] != null )
            {
                LogOptions = (ServiceLogEventOptions)config.User[path];
            }
        }
        private void ProcessLogOptions( ServiceLogEventOptions e )
        {
            DoLogErrors =                        ((e & ServiceLogEventOptions.LogErrors) == ServiceLogEventOptions.LogErrors);
            DoLogStartRaise =                    ((e & ServiceLogEventOptions.StartRaise) == ServiceLogEventOptions.StartRaise);
            DoLogParameters =                    ((e & ServiceLogEventOptions.LogParameters) == ServiceLogEventOptions.LogParameters);
            DoLogEndRaise =                      ((e & ServiceLogEventOptions.EndRaise) == ServiceLogEventOptions.EndRaise);
            DoCatchEventWhenServiceStopped =     ((e & ServiceLogEventOptions.SilentEventRunningStatusError) == ServiceLogEventOptions.SilentEventRunningStatusError);
            DoLogCaughtEventWhenServiceStopped = ((e & ServiceLogEventOptions.LogSilentEventRunningStatusError) == ServiceLogEventOptions.LogSilentEventRunningStatusError);
            DoCatchBadEventHandling =            ((e & ServiceLogEventOptions.SilentEventError) == ServiceLogEventOptions.SilentEventError);
        }

        public ServiceLogEventOptions GetLogOptions()
        {
            ServiceLogEventOptions l = ServiceLogEventOptions.None;
            if( _doLogErrors )                          l = l | ServiceLogEventOptions.LogErrors;            
            if( _doLogStartRaise )                      l = l | ServiceLogEventOptions.StartRaise;
            if( _doLogParameters )                      l = l | ServiceLogEventOptions.LogParameters;
            if( _doLogEndRaise )                        l = l | ServiceLogEventOptions.EndRaise;
            if( _doCatchEventWhenServiceStopped )       l = l | ServiceLogEventOptions.SilentEventRunningStatusError;
            if( _doLogCaughtEventWhenServiceStopped )   l = l | ServiceLogEventOptions.LogSilentEventRunningStatusError;
            if( _doCatchBadEventHandling )              l = l | ServiceLogEventOptions.SilentEventError;
          
            return l;
        }

        #region Commented

        //private void ProcessLogEventErrorFilter(LogEventErrorFilter e)
        //{
        //    DoLogErrors = ((e & LogEventErrorFilter.Log) == LogEventErrorFilter.Log);
        //    DoProtect = ((e & LogEventErrorFilter.Protect) == LogEventErrorFilter.Protect);
        //}

        //public LogEventErrorFilter GetErrorFilter()
        //{
        //    LogEventErrorFilter l = new LogEventErrorFilter();
        //    if (_doLogErrors) l = l | LogEventErrorFilter.Log;
        //    if (_doProtect) l = l | LogEventErrorFilter.Protect;
        //    return l;
        //}

        //private void ProcessLogEventFilter(LogEventFilter e)
        //{
        //    DoLogCaller = ((e & LogEventFilter.Caller) == LogEventFilter.Caller);
        //    DoLogParameters = ((e & LogEventFilter.Parameters) == LogEventFilter.Parameters);
        //    DoLogDelegates = ((e & LogEventFilter.Delegates) == LogEventFilter.Delegates);
        //}

        //private LogEventFilter GetEventFilter()
        //{
        //    LogEventFilter l = new LogEventFilter();
        //    if (_doLogCaller) l = l | LogEventFilter.Caller;
        //    if (_doLogParameters) l = l | LogEventFilter.Parameters;
        //    if (_doLogDelegates) l = l | LogEventFilter.Delegates;
        //    return l;
        //}

        #endregion

        /// <summary>
        /// Sets every log configuration to false
        /// </summary>
        public void ClearConfig()
        {
            DoLog = false;
            DoLogErrors = false;
            DoLogStartRaise = false;
            DoLogParameters = false;
            DoLogEndRaise = false;
            DoCatchEventWhenServiceStopped = false;
            DoLogCaughtEventWhenServiceStopped = false;
            DoCatchBadEventHandling = false;
        }

        #endregion              
    
        
    }
}
