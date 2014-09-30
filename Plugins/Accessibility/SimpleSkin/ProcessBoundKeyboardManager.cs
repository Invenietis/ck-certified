using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Threading;
using System.Xml;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Plugin.Config.Model;
using CK.Storage;
using CK.Windows;
using CommonServices;

namespace SimpleSkin
{
    [Plugin( ProcessBoundKeyboardManager.PluginIdString, PublicName = PluginPublicName, Version = ProcessBoundKeyboardManager.PluginIdVersion,
       Categories = new string[] { "Visual", "Accessibility" } )]
    public class ProcessBoundKeyboardManager : IPlugin
    {
        const string PluginPublicName = "ProcessBoundKeyboardManager";
        const string PluginIdString = "{A6E29D3A-4376-4DD7-AA4C-3A77EBEE13AF}";
        const string PluginIdVersion = "1.0.0";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IProcessMonitor> ProcessMonitorService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        IKeyboard _previousCurrentKeyboard;
        public string _previousForegroundProcessName;

        Dictionary<IKeyboard, ProcessBoundKeyboardConfig> _currentKeyboardConfigs;
        Dictionary<string, List<ProcessBoundKeyboardConfig>> _processNamesToKeyboardConfigs;

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            _currentKeyboardConfigs = new Dictionary<IKeyboard, ProcessBoundKeyboardConfig>();
            _processNamesToKeyboardConfigs = new Dictionary<string, List<ProcessBoundKeyboardConfig>>();

            return true;
        }

        public void Start()
        {
            Config.ConfigChanged += Config_ConfigChanged;
            LoadConfig();

            ProcessMonitorService.Service.ForegroundProcessChanged += Service_ForegroundProcessChanged;
            ProcessMonitorService.Service.ProcessStarted += Service_ProcessStarted;
            ProcessMonitorService.Service.ProcessStopped += Service_ProcessStopped;
        }

        public void Stop()
        {
            SaveConfig();
            ProcessMonitorService.Service.ForegroundProcessChanged -= Service_ForegroundProcessChanged;
            ProcessMonitorService.Service.ProcessStarted -= Service_ProcessStarted;
            ProcessMonitorService.Service.ProcessStopped -= Service_ProcessStopped;
        }

        public void Teardown()
        {
        }

        #endregion

        void Config_ConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            LoadConfig();
        }

        /// <summary>
        /// Used to handle a keyboard config when a process starts/stops/changes foreground.
        /// </summary>
        /// <param name="e">Process name</param>
        /// <param name="DoProcess">Delegate to call after confirming
        /// that the process is registered with a keyboard and that the keyboard exists.</param>
        void HandleKeyboard( string processName, ProcessKeyboardConfig DoProcess )
        {
            List<ProcessBoundKeyboardConfig> keyboardConfigs;
            if( _processNamesToKeyboardConfigs.TryGetValue( processName, out keyboardConfigs ) )
            {
                // Process is bound to keyboards.
                foreach( ProcessBoundKeyboardConfig config in keyboardConfigs )
                {
                    // Ensure that keyboard exists.
                    IKeyboard kb = KeyboardContext.Service.Keyboards[config.Keyboard];
                    if( kb == null ) continue;

                    DoProcess( kb, config );
                }
            }
        }

        delegate void ProcessKeyboardConfig( IKeyboard kb, ProcessBoundKeyboardConfig config );

        void Service_ProcessStopped( object sender, ProcessEventArgs e )
        {
            NoFocusManager.Default.ExternalDispatcher.Invoke( new Action( () =>
            {
                HandleKeyboard( e.ProcessName, ( kb, config ) =>
                {
                    /**
                     * The process was stopped.
                     * If DeactivateWithProcess is true, then we need to reset the keyboard to its previous state.
                     * */
                    if( config.DeactivateWithProcess )
                    {
                        DisableKeyboardOrResetPreviousCurrent( kb, config );
                    }
                } );
            } ) );
        }

        void Service_ProcessStarted( object sender, ProcessEventArgs e )
        {
            NoFocusManager.Default.ExternalDispatcher.Invoke( new Action( () =>
            {
                HandleKeyboard( e.ProcessName, ( kb, config ) =>
                {
                    /**
                     * The process was started.
                     * If KeepKeyboardWithProcessInBackground is enabled, then the keyboard should be enabled even if it's not in foreground.
                     * */

                    if( config.KeepKeyboardWithProcessInBackground )
                    {
                        EnableKeyboardOrSetAsCurrent( kb, config );
                    }
                } );
            } ) );
        }

        void Service_ForegroundProcessChanged( object sender, ProcessEventArgs e )
        {
            // Ignore where Civikey is concerned.
            if( e.Process.Id == Process.GetCurrentProcess().Id )
            {
                return;
            }

            NoFocusManager.Default.ExternalDispatcher.Invoke( new Action( () =>
            {

                if( _previousForegroundProcessName != null )
                {
                    OnLeaveForegroundProcess( _previousForegroundProcessName );
                }

                _previousForegroundProcessName = e.ProcessName;

                HandleKeyboard( e.ProcessName, ( kb, config ) =>
                {
                    /**
                     * Process went to foreground.
                     * If KeepKeyboardWithProcessInBackground is disabled, then we should enable accordingly.
                     * */
                    if( !config.KeepKeyboardWithProcessInBackground )
                    {
                        EnableKeyboardOrSetAsCurrent( kb, config );
                    }
                } );
            } ) );
        }

        void OnLeaveForegroundProcess( string previousForegroundProcess )
        {
            // Already in dispatcher
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            HandleKeyboard( previousForegroundProcess, ( kb, config ) =>
            {
                /**
                 * Process left foreground.
                 * If KeepKeyboardWithProcessInBackground is disabled, then we should disable accordingly.
                 * */
                if( !config.KeepKeyboardWithProcessInBackground )
                {
                    DisableKeyboardOrResetPreviousCurrent( kb, config );
                }
            } );
        }

        void EnableKeyboardOrSetAsCurrent( IKeyboard kb, ProcessBoundKeyboardConfig config )
        {
            if( config.UseAsMainKeyboard )
            {
                _previousCurrentKeyboard = KeyboardContext.Service.CurrentKeyboard;
                KeyboardContext.Service.CurrentKeyboard = kb;
            }
            else
            {
                kb.IsActive = true;
            }
        }

        void DisableKeyboardOrResetPreviousCurrent( IKeyboard kb, ProcessBoundKeyboardConfig config )
        {
            if( config.UseAsMainKeyboard && _previousCurrentKeyboard != null )
            {
                KeyboardContext.Service.CurrentKeyboard = _previousCurrentKeyboard;
            }
            else if( !config.UseAsMainKeyboard )
            {
                kb.IsActive = false;
            }
        }

        void LoadConfig()
        {
            _currentKeyboardConfigs.Clear();
            ProcessBoundKeyboardCollectionConfig configs = Config.User.GetOrSet( "ProcessBoundKeyboardCollectionConfig", new ProcessBoundKeyboardCollectionConfig() );

            //(ProcessBoundKeyboardCollectionConfig)Config.User["ProcessBoundKeyboardCollectionConfig"];

            foreach( var config in configs.Keyboards )
            {
                // Won't work if config has empty value.
                if( String.IsNullOrEmpty( config.Keyboard ) ) continue;

                // Only add when keyboard exists.
                IKeyboard kb = KeyboardContext.Service.Keyboards[config.Keyboard];
                if( kb != null )
                {
                    _currentKeyboardConfigs.Add( kb, config );
                }
            }

            BuildReverseDictionary();
        }

        void SaveConfig()
        {
            ProcessBoundKeyboardCollectionConfig configs = new ProcessBoundKeyboardCollectionConfig();

            configs.Keyboards.AddRange( _currentKeyboardConfigs.Values );

            ChangeStatus status = Config.User.Set( "ProcessBoundKeyboardCollectionConfig", configs );

        }

        void BuildReverseDictionary()
        {
            // Since we receive -- and only receive -- process names, we need to associate them to keyboards
            // and their configuration for easy access.
            // That's why we build a reverse dictionary.

            _processNamesToKeyboardConfigs.Clear();
            foreach( ProcessBoundKeyboardConfig config in _currentKeyboardConfigs.Values )
            {
                foreach( string processName in config.BoundProcessNames )
                {
                    if( !_processNamesToKeyboardConfigs.ContainsKey( processName ) )
                    {
                        _processNamesToKeyboardConfigs.Add( processName, new List<ProcessBoundKeyboardConfig>() );
                    }

                    _processNamesToKeyboardConfigs[processName].Add( config );
                }
            }
        }
    }


    public class ProcessBoundKeyboardCollectionConfig : IStructuredSerializable
    {
        List<ProcessBoundKeyboardConfig> _keyboards = new List<ProcessBoundKeyboardConfig>();
        public List<ProcessBoundKeyboardConfig> Keyboards { get { return _keyboards; } }

        #region IStructuredSerializable Members

        public void ReadContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            r.Read();
            r.ReadStartElement( "ProcessBoundKeyboardCollectionConfig" );
            if( r.IsStartElement( "ProcessBoundKeyboardConfig" ) )
            {
                while( r.IsStartElement( "ProcessBoundKeyboardConfig" ) )
                {
                    ProcessBoundKeyboardConfig config = new ProcessBoundKeyboardConfig();
                    sr.ReadInlineObjectStructuredElement( "ProcessBoundKeyboardConfig", config );
                    _keyboards.Add( config );
                }
            }

            r.ReadEndElement();
        }

        public void WriteContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            w.WriteStartElement( "ProcessBoundKeyboardCollectionConfig" );

            foreach( ProcessBoundKeyboardConfig config in _keyboards )
            {
                sw.WriteInlineObjectStructuredElement( "ProcessBoundKeyboardConfig", config );
            }

            w.WriteFullEndElement();
        }

        #endregion
    }

    /// <summary>
    /// Configuration for Keyboards that can bind to certain processes.
    /// </summary>
    public class ProcessBoundKeyboardConfig : IStructuredSerializable
    {
        /// <summary>
        /// Whether to deactivate this keyboard when the process leaves foreground.
        /// 
        /// If KeepKeyboardInBackground is enabled, the keyboard will be deactivated when the process quits instead.
        /// If UseAsMainKeyboard is enabled, this will reset the previous <see cref="IKeyboardCollection.Current"/> keyboard instead.
        /// </summary>
        public bool DeactivateWithProcess { get; set; }

        /// <summary>
        /// Whether to keep the keyboard activated in background when the keyboard's process leaves foreground, while the process is still active.
        /// 
        /// If true, this will still cause the keyboard to be deactivated when the process is stopped.
        /// </summary>
        public bool KeepKeyboardWithProcessInBackground { get; set; }

        /// <summary>
        /// Whether to set the keyboard as a main keyboard (<see cref="IKeyboardCollection.Current"/>),
        /// instead of just activating it as an additional keyboard (<see cref="IKeyboard.IsActive"/>).
        /// </summary>
        public bool UseAsMainKeyboard { get; set; }

        string _keyboard;
        public string Keyboard { get { return _keyboard; } }

        readonly List<string> _boundProcessNames;
        public IList<string> BoundProcessNames { get { return _boundProcessNames; } }

        public ProcessBoundKeyboardConfig( string keyboardName = null, IEnumerable<string> processNames = null )
        {
            _keyboard = keyboardName;

            _boundProcessNames = new List<string>();
            if( processNames != null )
            {
                _boundProcessNames.AddRange( processNames );
            }
        }

        #region IStructuredSerializable Members

        public void ReadContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;

            string keyboardName = r.GetAttribute( "KeyboardName" );
            _keyboard = keyboardName;

            DeactivateWithProcess = r.GetAttributeBoolean( "DeactivateWithProcess", false );
            KeepKeyboardWithProcessInBackground = r.GetAttributeBoolean( "KeepKeyboardWithProcessInBackground", false );
            UseAsMainKeyboard = r.GetAttributeBoolean( "UseAsMainKeyboard", false );


            using( XmlReader r2 = r.ReadSubtree() )
            {
                while( r2.Read() )
                {
                    switch( r2.NodeType )
                    {
                        case XmlNodeType.Element:
                            if( r2.Name == "ProcessNames" )
                            {
                                using( XmlReader r3 = r.ReadSubtree() )
                                {
                                    while( r3.Read() )
                                    {
                                        switch( r3.NodeType )
                                        {
                                            case XmlNodeType.Element:
                                                if( r3.Name == "ProcessName" )
                                                {
                                                    string processName = r3.ReadElementContentAsString();
                                                    _boundProcessNames.Add( processName );
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }

        }

        public void WriteContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;

            w.WriteAttributeString( "KeyboardName", _keyboard );

            w.WriteAttributeString( "DeactivateWithProcess", DeactivateWithProcess.ToString().ToLowerInvariant() );
            w.WriteAttributeString( "KeepKeyboardWithProcessInBackground", KeepKeyboardWithProcessInBackground.ToString().ToLowerInvariant() );
            w.WriteAttributeString( "UseAsMainKeyboard", UseAsMainKeyboard.ToString().ToLowerInvariant() );

            w.WriteStartElement( "ProcessNames" );

            foreach( string processName in _boundProcessNames )
            {
                w.WriteElementString( "ProcessName", processName );
            }

            w.WriteFullEndElement();
        }

        #endregion
    }
}
