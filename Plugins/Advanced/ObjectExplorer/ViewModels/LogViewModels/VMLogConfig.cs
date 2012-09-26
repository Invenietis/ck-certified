#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\LogViewModels\VMLogConfig.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Linq;
using CK.Plugin;
using CK.Core;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CK.WPF.ViewModel;
using System.ComponentModel;
using System.Diagnostics;
using CK.Plugin.Hosting;
using CK.Plugin.Config;
using CommonServices;
using System.Collections.Generic;

namespace CK.Plugins.ObjectExplorer.ViewModels.LogViewModels
{
    public class VMLogConfig : VMISelectableElement, ILogConfig
    {
        const string GLOBAL_LOGS = "GlobalLogs";
        const string OUTPUT_MAX_COUNT = "OutputMaxCount";

        #region Variables & Properties

        ObservableCollection<VMLogServiceConfig> _services;
        IReadOnlyCollection<ILogServiceConfig> _servicesEx;
        ServiceHostConfiguration _hostConfiguration;
        VMLogServiceConfig _selectedService;
        VMIContextViewModel _vmiContext;
        PluginRunner _pluginRunner;
        ILogService _logService;

        bool _doLog;
        bool _isDirty;

        ICommand _applyCommand;
        ICommand _cancelCommand;

        public ObservableCollection<VMLogServiceConfig> Services { get { return _services; } }
        IReadOnlyCollection<ILogServiceConfig> ILogConfig.Services { get { return _servicesEx; } }
        public IPluginConfigAccessor Config { get { return VMIContext.Config; } }
        public VMLogOutputContainer LogEntriesContainer { get; private set; }
        public string Icon { get { return "../LogImages/LogIcon.png"; } }
        public object Data { get { return this; } }

        public VMLogServiceConfig SelectedService
        {
            get
            {
                if( _selectedService == null && Services.Count > 0 )
                    SelectedService = Services.ElementAt( 0 );
                return _selectedService;
            }
            set
            {
                if( _selectedService != value )
                {
                    _selectedService = value;
                    OnPropertyChanged( "SelectedService" );
                }
            }
        }
        public int NumberOfLoggedServices
        {
            get { return Services.Count( ( s ) => { return s.DoLog; } ); }
        }
        public ICommand CancelCommand
        {
            get
            {
                if( _cancelCommand == null )
                {
                    _cancelCommand = new VMCommand<VMLogConfig>(
                    ( e ) =>
                    {
                        CancelAllModifications();
                    } );
                }
                return _cancelCommand;
            }
        }
        public ICommand ApplyCommand
        {
            get
            {
                if( _applyCommand == null )
                {
                    _applyCommand = new VMCommand<VMLogConfig>(
                    ( e ) =>
                    {
                        Apply();
                    } );
                }
                return _applyCommand;
            }
        }
        bool ILogConfig.DoLog
        {
            get { return _doLog; }
        }
        public bool IsDirty
        {
            get
            {
                if( _isDirty )
                    return true;
                foreach( var s in Services )
                    if( s.IsDirty ) return true;
                return false;

                // return _isDirty || Services.Any((s) => s.IsDirty);
            }
            set { _isDirty = value; OnPropertyChanged( "IsDirty" ); }
        }
        public bool IsEmpty
        {
            get
            {
                foreach( var s in Services )
                    if( !s.IsEmpty ) return false;
                return true;

                // return !Services.Any((s) => !s.IsEmpty);
            }
        }
        public bool DoLog
        {
            get { return _doLog; }
            set
            {
                _doLog = value;
                OnPropertyChanged( "DoLog" );
                IsDirty = true;
            }
        }
 
        #endregion

        #region Constructor

        /// <summary>
        /// Instanciates a VMLogConfig, without populating it
        /// you can populate it by calling its Initialize() method        
        /// </summary>
        /// <param name="ctx"></param>
        public VMLogConfig( VMIContextViewModel ctx, ILogService logService )
            : base( ctx, null )
        {
            _vmiContext = ctx;
            _services = new ObservableCollection<VMLogServiceConfig>();
            _servicesEx = new ReadOnlyCollectionTypeConverter<ILogServiceConfig, VMLogServiceConfig>( _services, ( c ) => { return (ILogServiceConfig)c; } );
            _logService = logService;
            LogEntriesContainer = new VMLogOutputContainer();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Launches Initialization of the VMLogConfig
        /// </summary>
        public void Initialize()
        {
            _services.Clear();
            FillFromDiscoverer( _services );

            LogEntriesContainer.MaxCount = Config.User.GetOrSet( OUTPUT_MAX_COUNT, 100 );
            DoLog = Config.User.GetOrSet( GLOBAL_LOGS, true );

            _hostConfiguration = new ServiceHostConfiguration( this );
            _pluginRunner = _vmiContext.Context.GetService<PluginRunner>( true );
            _vmiContext.Context.PluginRunner.ServiceHost.Add( _hostConfiguration );

            Apply();

            if( _logService != null ) //The service is set as "Optional TryStart", prevent any null ref exception
                _logService.LogTriggered += new LogTriggeredEventHandler( OnLogTriggered );
        }

        /// <summary>
        /// Initializes the VMLogConfig from Loader info
        /// </summary>
        internal void FillFromDiscoverer( ObservableCollection<VMLogServiceConfig> services )
        {
            PluginRunner pluginRunner = _vmiContext.Context.GetService<PluginRunner>( true );

            foreach( IServiceInfo s in pluginRunner.Discoverer.Services )
            {
                if( s.IsDynamicService )
                {
                    VMLogServiceConfig vmS = VMLogServiceConfig.CreateFrom( this, s );
                    EventRegistration( vmS );
                    services.Add( vmS );
                }
            }
            //pluginRunner.NewPluginsLoaded += new EventHandler(OnNewPluginsLoaded);
        }

        /// <summary>
        /// Clears the whole log configuration and pastes the Kernel's log configuration
        /// </summary>
        internal void UpdateFromHostConfiguration()
        {
            UpdateFrom( _hostConfiguration.LogConfig, false );
        }

        /// <summary>
        /// Updates all VMLogServiceConfig to reflect the Log configuration set as parameter
        /// If this method if called in response to a ConfigCanged from the interceptor, we don't track changes
        /// If this method is called in response to a LoadXMLConfig, we do track changes
        /// </summary>
        /// <param name="l"></param>
        /// <param name="isDirty"></param>
        internal void UpdateFrom( ILogConfig l, bool trackChanges )
        {
            bool hasChanged = false;

            foreach( ILogServiceConfig s in l.Services )
            {
                VMLogServiceConfig vmS;

                vmS = Find( s.Name );
                if( vmS != null ) //The vm already exists
                {
                    vmS.UpdateFrom( s, trackChanges );
                }
                else
                {
                    vmS = VMLogServiceConfig.CreateFrom( this, s );
                    EventRegistration( vmS );
                    vmS.UpdateFrom( s, trackChanges );
                    if( trackChanges ) vmS.IsDirty = true; //If we create a new service from another source than the kernel, it must be set to dirty
                    _services.Add( vmS );
                }
            }
            if( l.DoLog != DoLog )
            {
                DoLog = l.DoLog;
                hasChanged = true;
            }

            if( !trackChanges ) IsDirty = false; // If we update from the kernel, conf is no longer dirty
            else if( hasChanged ) IsDirty = true; // If we update from an other source and changes have been made in the logConfig's cocnf, conf is dirty. But if no changes have been made, conf stays the way it was before updating

            if( !trackChanges )
                Debug.Assert( IsDirty == false );
        }

        internal void UpdateFrom( IPluginConfigAccessor config )
        {
            if( config.User[GLOBAL_LOGS] != null )
            {
                DoLog = (bool)config.User[GLOBAL_LOGS];
            }
            foreach( VMLogServiceConfig service in Services )
            {
                service.UpdateFrom( config );
            }
        }

        /// <summary>
        /// Clears the whole config and updates it from the kernel's configuration
        /// </summary>
        internal void CancelAllModifications()
        {
            ClearConfig();
            UpdateFromHostConfiguration();
        }

        /// <summary>
        /// Cancels modifications on a particular service
        /// if the service is bound or is not bound but can be found in the kernel's configuration, clears it and updates it from the kernel's configuration
        /// if the service is not bound, and can't be found in the kernel's configuration, removes it
        /// </summary>
        /// <param name="vmS"></param>
        internal void CancelModifications( VMLogServiceConfig vmS )
        {
            vmS.ClearConfig();

            if( vmS.IsBound )
            {

                foreach( ILogServiceConfig s in _hostConfiguration.LogConfig.Services )
                {
                    if( s.Name == vmS.Name )
                    {
                        vmS.UpdateFrom( s, false );// We update from the kernel, we don't track changes (and therefor set IsDirty to false)
                        break;
                    }
                }
            }
            else
            {
                bool hasBeenFound = false;
                foreach( ILogServiceConfig s in _hostConfiguration.LogConfig.Services )
                {
                    if( s.Name == vmS.Name )
                    {
                        vmS.UpdateFrom( s, false );// We update from the kernel, we don't track changes (and therefor set IsDirty to false)
                        _services.Add( vmS );
                        hasBeenFound = true;
                        break;
                    }
                }
                if( !hasBeenFound )
                    _services.Remove( vmS );
            }
        }

        /// <summary>
        /// This methods calls the interceptor's Apply method
        /// </summary>
        internal void Apply()
        {
            ILogConfig l = ( (ILogConfig)this ).Clone();
            _hostConfiguration.ApplyConfiguration( l );
            _pluginRunner.ServiceHost.ApplyConfiguration();

            _isDirty = false;
            Config.User.Set( GLOBAL_LOGS, DoLog );

            foreach( VMLogServiceConfig s in Services )
            {
                s.UpdatePropertyBag();
                s.IsDirty = false;
            }
        }

        /// <summary>
        /// Sets back an "after InitializeFromLoader" state - state in which we only have empty bound services/methods/events/properties
        /// removes every unbound service.
        /// </summary>
        internal void ClearConfig()
        {
            DoLog = false;
            for( int i = 0; i < _services.Count; ++i )
            {
                VMLogServiceConfig s = _services[i];
                if( s.IsBound ) s.ClearConfig();
                else _services.RemoveAt( i-- );
            }
        }

        /// <summary>
        /// Called when the log configuration of the Kernel has changed
        /// Launches UpdateFromKernel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogConfigChanged( object sender, EventArgs e )
        {
            ClearConfig();
            UpdateFromHostConfiguration();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogServiceConfigChanged( object sender, EventArgs e )
        {
            ILogServiceConfig s = (ILogServiceConfig)sender;
            Find( s.Name ).UpdateFrom( s, false );
        }

        /// <summary>
        /// Similiar to a GoTo, after the method is called, the UI presents the Service UI that sent the ServiceModifictionAsked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServiceModificationAsked( object sender, EventArgs e )
        {
            SelectedService = (VMLogServiceConfig)sender;
            VMIContext.SelectedElement = this;
        }

        /// <summary>
        /// Launches Initialize() when the loader has been re-loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNewPluginsLoaded( object sender, EventArgs e )
        {
            Initialize();
        }

        /// <summary>
        /// Cancels modifications on a given Service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServiceCancelModificationsAsked( object sender, EventArgs e )
        {
            CancelModifications( (VMLogServiceConfig)sender );
        }

        /// <summary>
        /// Deletes the unbound VMLogServiceConfig that has sent the ServiceDeletionAsked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServiceDeletionAsked( object sender, EventArgs e )
        {
            VMLogServiceConfig s = (VMLogServiceConfig)sender;
            if( !s.IsBound )
                Services.Remove( s );
        }

        /// <summary>
        /// Called when something changes in one of the services
        /// Tells the view to refresh the LogConfig's IsDirty Property, to show that The config is dirty when one of its service is dirty.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServiceLogConfigChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsDirty" ) OnPropertyChanged( "IsDirty" );
            if( e.PropertyName == "DoLog" ) OnPropertyChanged( "NumberOfLoggedServices" );
        }

        /// <summary>
        /// Called when a service wants its log configuration to be applied to the interceptor's configuration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void ServiceApply( VMLogServiceConfig service )
        {
            if( service != null )
            {
                _hostConfiguration.ApplyConfiguration( service );
                _pluginRunner.ServiceHost.ApplyConfiguration();
                service.IsDirty = false;
            }
        }

        /// <summary>
        /// Registers the config to the event from the vmLogServiceConfig it needs to listen to
        /// </summary>
        /// <param name="s"></param>
        internal void EventRegistration( VMLogServiceConfig s )
        {
            s.ServiceCancelModificationsAsked += new EventHandler( OnServiceCancelModificationsAsked );
            s.ServiceDeletionAsked += new EventHandler( OnServiceDeletionAsked );
            s.LogConfigChanged += new EventHandler<PropertyChangedEventArgs>( OnServiceLogConfigChanged );
            s.ServiceModificationAsked += new EventHandler( OnServiceModificationAsked );
        }

        /// <summary>
        /// Returns the VMLogServiceConfig corresponding to the name set as parameter
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public VMLogServiceConfig Find( string name )
        {
            foreach( VMLogServiceConfig vmLogServiceConfig in _services )
            {
                if( vmLogServiceConfig.Name == name )
                    return vmLogServiceConfig;
            }
            return null;
        }

        /// <summary>
        /// Pushes logs into the output console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnLogTriggered( object sender, LogTriggeredEventArgs e )
        {
            LogEntriesContainer.Add( e.LogEventArgs, e.Content );
        }

        #endregion

    }
}
