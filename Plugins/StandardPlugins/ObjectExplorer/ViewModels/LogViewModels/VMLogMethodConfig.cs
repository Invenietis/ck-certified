using System.Collections.Generic;
using System.Reflection;
using CK.Core;
using System;
using CK.Plugin.Hosting;
using CK.Plugin;
using CK.Plugin.Config;

namespace CK.StandardPlugins.ObjectExplorer.ViewModels.LogViewModels
{
    public class VMLogMethodConfig : VMLogBaseElement, ILogMethodConfig
    {

        #region CreateFrom Methods

        public static VMLogMethodConfig CreateFrom( VMLogServiceConfig holder, ISimpleMethodInfo m )
        {
            VMLogMethodConfig result = new VMLogMethodConfig( holder, m.Name, true );

            result.ReturnType = m.ReturnType.ToString();
            foreach( ISimpleParameterInfo p in m.Parameters )
            {
                result.Parameters.Add( new LogParameterInfo( p.ParameterName, p.ParameterType ) );
            }

            result._holder = holder;
            result._dataPath = result._holder.Name + "_" + result.FullSignature;

            //If there is no config, we set the default one.
            if( result._holder.Config.User[result._dataPath + "_logoptions"] == null ) result._doLogErrors = true;
            else result.LogOptions = (ServiceLogMethodOptions)result._holder.Config.User[result._dataPath + "_logoptions"];

            if( holder.Config.User[result._dataPath + "_globaldolog"] == null ) result._doLog = true;
            else result._doLog = (bool)result._holder.Config.User[result._dataPath + "_globaldolog"];

            return result;
        }

        public static VMLogMethodConfig CreateFrom( VMLogServiceConfig holder, ILogMethodConfig m )
        {
            VMLogMethodConfig result = new VMLogMethodConfig( holder, m.Name, false );
            result._doLog = m.DoLog;
            result.ReturnType = m.ReturnType.ToString();
            foreach( ILogParameterInfo p in m.Parameters )
                result.Parameters.Add( new LogParameterInfo( p.ParameterName, p.ParameterType ) );
            result.LogOptions = m.LogOptions;
            return result;
        }

        #endregion

        #region Properties

        VMLogServiceConfig _holder;
        List<ILogParameterInfo> _parameters;
        IReadOnlyList<ILogParameterInfo> _parametersEx;
        string _returnType;
        bool _doLogErrors;
        bool _doLogEnter;
        bool _doLogParameters;
        bool _doLogCaller;
        bool _doLogLeave;
        bool _doLogReturnValue;

        string _dataPath;

        public string FullSignature
        {
            get { return this.GetSimpleSignature(); }
        }
        public string CleanSignature
        {
            get { return this.GetCleanSignature(); }
        }

        public bool DoLogErrors
        {
            get { return _doLogErrors; }
            set { _doLogErrors = value; OnPropertyChanged( "DoLogErrors" ); OnLogConfigChanged( "DoLogErrors" ); }
        }
        public bool DoLogEnter
        {
            get { return _doLogEnter; }
            set { _doLogEnter = value; OnPropertyChanged( "DoLogEnter" ); OnLogConfigChanged( "DoLogEnter" ); }
        }
        public bool DoLogParameters
        {
            get { return _doLogParameters; }
            set { _doLogParameters = value; OnPropertyChanged( "DoLogParameters" ); OnLogConfigChanged( "DoLogParameters" ); }
        }
        public bool DoLogCaller
        {
            get { return _doLogCaller; }
            set { _doLogCaller = value; OnPropertyChanged( "DoLogCaller" ); OnLogConfigChanged( "DoLogCaller" ); }
        }
        public bool DoLogLeave
        {
            get { return _doLogLeave; }
            set { _doLogLeave = value; OnPropertyChanged( "DoLogLeave" ); OnLogConfigChanged( "DoLogLeave" ); }
        }
        public bool DoLogReturnValue
        {
            get { return _doLogReturnValue; }
            set { _doLogReturnValue = value; OnPropertyChanged( "DoLogReturnValue" ); OnLogConfigChanged( "DoLogReturnValue" ); }
        }

        public VMLogServiceConfig Holder { get { return _holder; } }
        public IPluginConfigAccessor Config { get; set; }
        public bool IsEmpty { get { return !(_doLogEnter | _doLogParameters | _doLogCaller | _doLogLeave | _doLogReturnValue | _doLog); } }
        Core.IReadOnlyList<ILogParameterInfo> ILogMethodConfig.Parameters
        {
            get { return _parametersEx; }
        }
        public List<ILogParameterInfo> Parameters { get { return _parameters; } }
        public string ReturnType { get { return _returnType; } private set { _returnType = value; } }
        public ServiceLogMethodOptions LogOptions
        {
            get { return GetLogOptions(); }
            private set { ProcessLogOptions( value ); }
        }

        #endregion

        #region Constructors

        public VMLogMethodConfig( VMLogServiceConfig holder, string name, bool isBound )
            : this( holder, name, "", new List<ILogParameterInfo>(), isBound )
        {          
        }

        internal VMLogMethodConfig( VMLogServiceConfig holder, string name, string returnType, List<ILogParameterInfo> parameters, bool isBound )
            : base( name, isBound )
        {
            _holder = holder; 
            _returnType = returnType;
            _parameters = parameters;
            _parametersEx = new ReadOnlyListTypeAdapter<ILogParameterInfo, ILogParameterInfo>( _parameters );            
        }
        #endregion

        #region Methods

        internal void UpdatePropertyBag()
        {
            _holder.Config.User[_dataPath + "_logoptions"] = GetLogOptions();
            _holder.Config.User[_dataPath + "_globaldolog"] = DoLog;
        }

        public bool UpdateFrom( ILogMethodConfig m )
        {
            bool hasChanged = false;

            if( m.LogOptions != LogOptions )
            {
                ProcessLogOptions( m.LogOptions );
                hasChanged = true;
            }

            if( m.DoLog != DoLog )
            {
                DoLog = m.DoLog;
                hasChanged = true;
            }

            return hasChanged;
        }

        public void UpdateFrom( IPluginConfigAccessor config )
        {
            string path = this.Name + "_logoptions";
            if( config.User[path] != null )
            {
                LogOptions = (ServiceLogMethodOptions)config.User[path];
            }
        }

        public void ClearConfig()
        {
            DoLog = false;
            DoLogErrors = false;
            DoLogEnter = false;
            DoLogCaller = false;
            DoLogParameters = false;
            DoLogLeave = false;
            DoLogReturnValue = false;

        }

        private void ProcessLogOptions( ServiceLogMethodOptions logOptions )
        {
            DoLogErrors =       ((logOptions & ServiceLogMethodOptions.LogError) == ServiceLogMethodOptions.LogError);
            DoLogEnter =        ((logOptions & ServiceLogMethodOptions.Enter) == ServiceLogMethodOptions.Enter);
            DoLogParameters =   ((logOptions & ServiceLogMethodOptions.LogParameters) == ServiceLogMethodOptions.LogParameters);
            DoLogCaller =       ((logOptions & ServiceLogMethodOptions.LogCaller) == ServiceLogMethodOptions.LogCaller);
            DoLogLeave =        ((logOptions & ServiceLogMethodOptions.Leave) == ServiceLogMethodOptions.Leave);
            DoLogReturnValue =  ((logOptions & ServiceLogMethodOptions.LogReturnValue) == ServiceLogMethodOptions.LogReturnValue);
        }

        private ServiceLogMethodOptions GetLogOptions()
        {
            ServiceLogMethodOptions l = ServiceLogMethodOptions.None;

            if( _doLogErrors )      l = l | ServiceLogMethodOptions.LogError;
            if( _doLogEnter )       l = l | ServiceLogMethodOptions.Enter;
            if( _doLogParameters )  l = l | ServiceLogMethodOptions.LogParameters;
            if( _doLogCaller )      l = l | ServiceLogMethodOptions.LogCaller;
            if( _doLogLeave )       l = l | ServiceLogMethodOptions.Leave;
            if( _doLogReturnValue ) l = l | ServiceLogMethodOptions.LogReturnValue;

            return l;
        }

        #endregion
    }
}