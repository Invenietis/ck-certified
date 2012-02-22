using System;
using CK.Plugin;
using System.Reflection;
using System.Collections.ObjectModel;
using CK.Core;
using CK.WPF.ViewModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;
using CK.Plugin.Config;

namespace CK.Plugins.ObjectExplorer.ViewModels.LogViewModels
{
    public class VMLogServiceConfig : VMLogBaseElement, ILogServiceConfig
    {

        #region CreateFrom Methods

        public static VMLogServiceConfig CreateFrom( VMLogConfig holder, IServiceInfo s )
        {
            VMLogServiceConfig result = new VMLogServiceConfig( s.ServiceFullName, true );
            result._holder = holder;
            result.Config = result._holder.Config;

            result.DoLog = result.Config.User.GetOrSet( result._doLogDataPath, true );

            foreach( ISimpleEventInfo e in s.EventsInfoCollection )
            {
                VMLogEventConfig evVM = VMLogEventConfig.CreateFrom( result, e );
                evVM.LogConfigChanged += new EventHandler<PropertyChangedEventArgs>(
                    ( sndr, args ) =>
                    { result.IsDirty = true; } //When an event's conf is modified, the service conf is set to dirty
                );
                result.Events.Add( evVM );
            }

            foreach ( ISimplePropertyInfo p in s.PropertiesInfoCollection )
            {
                VMLogPropertyConfig propVM = VMLogPropertyConfig.CreateFrom( p );
                propVM.LogConfigChanged += new EventHandler<PropertyChangedEventArgs>(
                    (sndr, args) => 
                        { result.IsDirty = true; }//When a property's conf is modified, the service conf is set to dirty
                    );
                result.Properties.Add( propVM );
            }

            foreach ( ISimpleMethodInfo m in s.MethodsInfoCollection )
            {
                VMLogMethodConfig mthdVM = VMLogMethodConfig.CreateFrom( result, m );
                mthdVM.LogConfigChanged += new EventHandler<PropertyChangedEventArgs>(
                (sndr, args) =>
                    { result.IsDirty = true; } //When a method's conf is modified, the service conf is set to dirty
                );
                result.Methods.Add(mthdVM);                
            }

            return result;
        }

        public static VMLogServiceConfig CreateFrom( VMLogConfig holder, ILogServiceConfig s )
        {
            VMLogServiceConfig result = new VMLogServiceConfig( s.Name, false );

            result._holder = holder;
            result.Config = holder.Config;
            result._doLog = s.DoLog;

            foreach( ILogEventConfig e in s.Events )
            {
                VMLogEventConfig evVM = VMLogEventConfig.CreateFrom( result, e );
                evVM.LogConfigChanged += new EventHandler<PropertyChangedEventArgs>(
                    ( sndr, args ) =>
                    { result.IsDirty = true; }
                    );
                result.Events.Add( evVM );
            }

            foreach( ILogPropertyConfig p in s.Properties )
            {
                VMLogPropertyConfig propVM = VMLogPropertyConfig.CreateFrom( p );
                propVM.LogConfigChanged += new EventHandler<PropertyChangedEventArgs>(
                    ( sndr, args ) =>
                    { result.IsDirty = true; }
                    );
                result.Properties.Add( propVM );
            }

            foreach( ILogMethodConfig m in s.Methods )
            {
                VMLogMethodConfig mthdVM = VMLogMethodConfig.CreateFrom(result, m );
                mthdVM.LogConfigChanged += new EventHandler<PropertyChangedEventArgs>(
                ( sndr, args ) =>
                { result.IsDirty = true; }
                );
                result.Methods.Add( mthdVM );
            }

            return result;
        }

        #endregion

        #region Properties & Variables

        VMLogConfig _holder;
        public event EventHandler ServiceModificationAsked;
        public event EventHandler ServiceCancelModificationsAsked;
        public event EventHandler ServiceDeletionAsked;
        public event EventHandler ServiceApplyAsked;
        bool _isDirty;
        string _dataPath;
        string _doLogDataPath;
        ICommand _applyCommand;
        ICommand _modifyCommand;
        ICommand _cancelCommand;
        ICommand _deleteCommand;        

        ObservableCollection<VMLogMethodConfig> _methods;
        IReadOnlyCollection<ILogMethodConfig> _methodsEx;

        ObservableCollection<VMLogEventConfig> _events;
        IReadOnlyCollection<ILogEventConfig> _eventsEx;

        ObservableCollection<VMLogPropertyConfig> _properties;
        IReadOnlyCollection<ILogPropertyConfig> _propertiesEx;

        public ObservableCollection<VMLogMethodConfig> Methods { get { return _methods; } }
        Core.IReadOnlyCollection<ILogMethodConfig> ILogServiceConfig.Methods
        {
            get { return _methodsEx; }
        }

        public ObservableCollection<VMLogEventConfig> Events { get { return _events; } }
        Core.IReadOnlyCollection<ILogEventConfig> ILogServiceConfig.Events
        {
            get { return _eventsEx; }
        }

        public ObservableCollection<VMLogPropertyConfig> Properties { get { return _properties; } }
        Core.IReadOnlyCollection<ILogPropertyConfig> ILogServiceConfig.Properties
        {
            get { return _propertiesEx; }
        }

        public VMLogConfig Holder { get { return _holder; } }
        public IPluginConfigAccessor Config { get; private set; }
        public string SimpleName
        {
            get
            {
                string temp = Name.Substring(Name.LastIndexOf('.')+1);
                if(temp.StartsWith("I"))
                    temp = temp.Remove(0,1);
                temp = temp.Replace("Service", "");
                return temp;
            }
        }
        public bool IsEmpty 
        { 
            get 
            {
                if (_doLog) return false;
                foreach (var m in Methods) if (!m.IsEmpty) return false;
                foreach (var e in Events) if (!e.IsEmpty) return false;
                foreach (var p in Properties) if (!p.IsEmpty) return false;
                return true;
            } 
        }        
        public bool IsDirty 
        {
            get { return _isDirty; }
            internal set 
            { 
                _isDirty = value; 
                OnPropertyChanged("IsDirty"); 
                OnLogConfigChanged("IsDirty"); 
            }
        }
        public override bool DoLog
        {
            get 
            { 
                return _doLog; 
            }
            set 
            { 
                _doLog = value; 
                IsDirty = true; 
                OnPropertyChanged("DoLog");
                OnLogConfigChanged("DoLog"); 
            }
        }

       
        public ICommand ApplyCommand
        {
            get
            {
                if (_applyCommand == null)
                {
                    _applyCommand = new VMCommand<VMLogServiceConfig>(
                    (e) =>
                    {
                        _holder.ServiceApply( this );

                    });
                }
                return _applyCommand;
            }
        }
        public ICommand ModifyCommand
        {
            get
            {
                if (_modifyCommand == null)
                {
                    _modifyCommand = new VMCommand<VMLogServiceConfig>(
                    (e) =>
                    {
                        if (e is VMLogServiceConfig)
                        {
                            if (ServiceModificationAsked != null)
                                ServiceModificationAsked(this,new EventArgs());
                        }
                    });
                }
                return _modifyCommand;
            }
        }        
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new VMCommand<VMLogServiceConfig>(
                    (e) =>
                    {
                        if (ServiceCancelModificationsAsked != null)
                            ServiceCancelModificationsAsked(this, new EventArgs());
                    });
                }
                return _cancelCommand;
            }
        }
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new VMCommand<VMLogServiceConfig>(
                    (e) =>
                    {
                        if (ServiceDeletionAsked != null)
                            ServiceDeletionAsked(this, new EventArgs());
                    });
                }
                return _deleteCommand;
            }
        }

        #endregion

        #region Constructors
       
        /// <summary>
        /// Instantiates an empty VMLogServiceConfig
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isBound"></param>
        public VMLogServiceConfig(string name, bool isBound)
            : base(name, isBound)
        {
            _dataPath = this.Name;
            _doLogDataPath = _dataPath + "-ServiceDoLog";

            _methods = new ObservableCollection<VMLogMethodConfig>();
            _methodsEx = new ReadOnlyCollectionTypeConverter<ILogMethodConfig, VMLogMethodConfig>( _methods, ( m ) => { return (ILogMethodConfig)m; } );

            _events = new ObservableCollection<VMLogEventConfig>();
            _eventsEx = new ReadOnlyCollectionTypeConverter<ILogEventConfig, VMLogEventConfig>( _events, ( m ) => { return (ILogEventConfig)m; } );

            _properties = new ObservableCollection<VMLogPropertyConfig>();
            _propertiesEx = new ReadOnlyCollectionTypeConverter<ILogPropertyConfig, VMLogPropertyConfig>( _properties, ( m ) => { return (ILogPropertyConfig)m; } );
        }

        #endregion

        #region Methods

        public void OutputConsoleUpdated()
        {
            OnPropertyChanged( "OutputText" );
        }

        internal void UpdatePropertyBag()
        {
            Config.User.Set( _doLogDataPath, DoLog );
            
            foreach( VMLogEventConfig e in Events )
            {
                e.UpdatePropertyBag();
            }
            foreach( VMLogMethodConfig m in Methods )
            {
                m.UpdatePropertyBag();
            }
            //foreach( VMLogPropertyConfig p in Properties )
            //{
            //    p.UpdateFrom( config );
            //}
        }

        /// <summary>
        /// Sets back an "after InitializeFromLoader" state - state in which we ony have empty bound sevices/methods/events/properties
        /// removes every unbound method/event/property.
        /// </summary>
        public void ClearConfig()
        {
            DoLog = false;
            for (int i = 0; i < _methods.Count; ++i)
            {
                VMLogMethodConfig c = _methods[i];
                if (c.IsBound) c.ClearConfig();
                else _methods.RemoveAt(i--);
            }

            for (int i = 0; i < _events.Count; ++i)
            {
                VMLogEventConfig c = _events[i];
                if (c.IsBound) c.ClearConfig();
                else _events.RemoveAt(i--);
            }

            for (int i = 0; i < _properties.Count; ++i)
            {
                VMLogPropertyConfig c = _properties[i];
                if (c.IsBound) c.ClearConfig();
                else _properties.RemoveAt(i--);
            }
        } 

        /// <summary>
        /// Updates all VMLogMethodConfig, VMLogEventConfig and VMLogPropertyConfig to reflect the Log service configuration set as parameter
        /// </summary>
        /// <param name="s"></param>
        public bool UpdateFrom(ILogServiceConfig s, bool trackChanges)
        {
            bool childHasChanged = false;
            bool hasChanged = false;
            VMLogMethodConfig vmMethod;

            foreach (ILogMethodConfig m in s.Methods)
            {
                vmMethod = FindMethod(m.GetSimpleSignature());
                if (vmMethod != null) //The VM exists
                {
                    childHasChanged = vmMethod.UpdateFrom(m);
                    if (childHasChanged) 
                        hasChanged = true;
                }
                else //The VM doesn't exist, create an unbound one 
                {
                    _methods.Add(VMLogMethodConfig.CreateFrom(this, m));
                    hasChanged = true;
                }
            }

            childHasChanged = false;

            VMLogEventConfig vmEvent;
            foreach (ILogEventConfig e in s.Events)
            {
                vmEvent = FindEvent(e.Name);
                if (vmEvent != null) //The VM exists  
                {
                    childHasChanged = vmEvent.UpdateFrom(e);
                    if (childHasChanged) 
                        hasChanged = true;
                }
                else //The VM doesn't exist, create an unbound one 
                {
                    _events.Add(VMLogEventConfig.CreateFrom(this, e));
                    hasChanged = true;
                }
            }

            childHasChanged = false;

            VMLogPropertyConfig vmProperty;
            foreach (ILogPropertyConfig p in s.Properties)
            {
                vmProperty = FindProperty(p.Name);
                if (vmProperty != null) //The VM exists  
                {
                    childHasChanged = vmProperty.UpdateFrom(p);
                    if (childHasChanged) 
                        hasChanged = true;
                }
                else //The VM doesn't exist, create an unbound one 
                {
                    _properties.Add(VMLogPropertyConfig.CreateFrom(p));
                    hasChanged = true;
                }
            }
            if (DoLog != s.DoLog)
            {
                hasChanged = true;
                DoLog = s.DoLog;
            }

            if (!trackChanges) // If we dont track changes, that's because we update from the kernel, we set IsDirty to false
                IsDirty = false;
            else if (hasChanged)
                IsDirty = true;

            return hasChanged;
        }

        public void UpdateFrom( IPluginConfigAccessor config )
        {
            string path = this.Name + "_dolog";
            if( config.User[path] != null )
            {
                DoLog = (bool)config.User["path"];
            }
            foreach( VMLogEventConfig e in Events )
            {
                e.UpdateFrom( config );
            }
            foreach( VMLogMethodConfig m in Methods )
            {
                m.UpdateFrom( config );
            }
            //foreach( VMLogPropertyConfig p in Properties )
            //{
            //    p.UpdateFrom( config );
            //}
        }

        /// <summary>
        /// Returns the VMLogMethodConfig corresponding to the signature set as parameter
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public VMLogMethodConfig FindMethod(string name)
        {
            foreach (VMLogMethodConfig vmMethod in Methods)
            {
                if (vmMethod.GetSimpleSignature() == name)
                    return vmMethod;
            }
            return null;
        }

        /// <summary>
        /// Returns the VMLogEventConfig corresponding to the name set as parameter
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public VMLogEventConfig FindEvent(string name)
        {
            foreach (VMLogEventConfig vmEvent in Events)
            {
                if (vmEvent.Name == name)
                    return vmEvent;
            }
            return null;
        }

        /// <summary>
        /// Returns the VMLogPropertyConfig corresponding to the name set as parameter
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public VMLogPropertyConfig FindProperty(string name)
        {
            foreach (VMLogPropertyConfig vmProperty in Properties)
            {
                if (vmProperty.Name == name)
                    return vmProperty;
            }
            return null;
        }

        #endregion       
    } 
}