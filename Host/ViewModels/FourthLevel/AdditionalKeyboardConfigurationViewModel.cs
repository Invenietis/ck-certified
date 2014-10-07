using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    /// <summary>
    /// Additional keyboard configuration config view.
    /// </summary>
    class AdditionalKeyboardConfigurationViewModel : ConfigBase
    {
        readonly IKeyboard _kb;
        readonly PluginCluster _pluginCluster;
        readonly string _radioGroupName;

        ConfigGroup _programGroup;

        SimpleSkin.ProcessBoundKeyboardConfig _config;
        ConfigImplementationSelectorItem _processBoundSelectorItem;
        ConfigImplementationSelectorItem _foregroundBoundSelectorItem;


        public bool KeepKeyboardWithProcessInBackground
        {
            get
            {
                return _config.KeepKeyboardWithProcessInBackground;
            }
            set
            {
                if( value != _config.KeepKeyboardWithProcessInBackground )
                {
                    _config.KeepKeyboardWithProcessInBackground = value;
                    SaveConfig();
                }
            }
        }

        public bool DeactivateWithProcess
        {
            get
            {
                return _config.DeactivateWithProcess;
            }
            set
            {
                if( value != _config.DeactivateWithProcess )
                {
                    _config.DeactivateWithProcess = value;
                    SaveConfig();
                }
            }
        }

        public bool UseAsMainKeyboard
        {
            get
            {
                return _config.UseAsMainKeyboard;
            }
            set
            {
                if( value != _config.UseAsMainKeyboard )
                {
                    _config.UseAsMainKeyboard = value;
                    SaveConfig();
                }
            }
        }

        /// <summary>
        /// ProcessBoundKeyboardManager configuration
        /// </summary>
        /// <param name="app"></param>
        /// <param name="kb"></param>
        public AdditionalKeyboardConfigurationViewModel( AppViewModel app, IKeyboard kb )
            : base( "{A6E29D3A-4376-4DD7-AA4C-3A77EBEE13AF}", R.KeyboardAutoLaunch, app )
        {
            Debug.Assert( kb != null );

            DisplayName = R.AdditionalKeyboardsDisplayName;

            _kb = kb;
            _pluginCluster = new PluginCluster( _app.PluginRunner, _app.CivikeyHost.Context.ConfigManager.UserConfiguration, new Guid( "{A6E29D3A-4376-4DD7-AA4C-3A77EBEE13AF}" ) );
            _radioGroupName = "KeyboardProgramTriggerBehaviorGroup";
        }

        protected override void OnInitialize()
        {
            this.Description = _kb.Name;

            this.Items.Add( new TitleItem( _app.ConfigManager, _kb.Name ) );
            this.Items.Add( new TextItem( _app.ConfigManager, R.KeyboardAutoActivationDescription, 12 ) );

            this.Items.Add( new TitleItem( _app.ConfigManager, R.BoundPrograms, 13 ) );
            _programGroup = this.AddGroup();

            this.AddAction( R.AddProgram, AddProcess );


            this.Items.Add( new TitleItem( _app.ConfigManager, R.Settings, 13 ) );
            CreateRadioButtonsGroup();

            this.AddProperty(
                R.DeactivateWithProcess,
                //R.DeactivateWithProcessDescription,
                this,
                (x => x.DeactivateWithProcess)
                );
            this.AddProperty(
                R.UseAsMainKeyboard,
                //R.UseAsMainKeyboardDescription,
                this,
                (x => x.UseAsMainKeyboard)
                );

            LoadConfig();
        }

        void CreateRadioButtonsGroup()
        {
            _processBoundSelectorItem = new ConfigImplementationSelectorItem(
                _app.ConfigManager,
                _pluginCluster,
                _radioGroupName
               );
            _processBoundSelectorItem.SelectAction = () => { KeepKeyboardWithProcessInBackground = true; };

            _processBoundSelectorItem.DisplayName = R.BindToProcess;
            _processBoundSelectorItem.Description = R.BindToProcessDescription;

            _foregroundBoundSelectorItem = new ConfigImplementationSelectorItem(
                _app.ConfigManager,
                _pluginCluster,
                _radioGroupName
               );
            _foregroundBoundSelectorItem.SelectAction = () => { KeepKeyboardWithProcessInBackground = false; };

            _foregroundBoundSelectorItem.DisplayName = R.BindToForegroundWindow;
            _foregroundBoundSelectorItem.Description = R.BindToForegroundWindowDescription;

            this.Items.Add( _foregroundBoundSelectorItem );
            this.Items.Add( _processBoundSelectorItem );
        }

        void UpdateRadioGroupStatus()
        {
            // Might be called during base construction because of OnConfigChanged, while we don't have the UI items. Ignore when this is the case.
            if( _foregroundBoundSelectorItem == null ) return;

            _foregroundBoundSelectorItem.IsSelected = !KeepKeyboardWithProcessInBackground;
            _processBoundSelectorItem.IsSelected = KeepKeyboardWithProcessInBackground;
        }

        void AddProcess()
        {
            string processName = SelectProcess();

            if( processName != null && !_config.BoundProcessNames.Contains( processName ) )
            {
                // SaveConfig will cause ConfigChanged to be triggered and call LoadConfig. Items will be added to the list there.
                _config.BoundProcessNames.Add( processName );
                SaveConfig();
            }
        }

        ConfigItem GenerateProcessConfigItem( string processName, Action removeClickAction )
        {
            TextItem textItem = new TextItem( _app.ConfigManager, processName, 12 );
            RemovableConfigItem removableItem = new RemovableConfigItem( _app.ConfigManager, textItem );

            removableItem.RemoveClick += ( s, e ) => { removeClickAction(); };

            return removableItem;
        }

        void LoadConfig()
        {
            SimpleSkin.ProcessBoundKeyboardCollectionConfig keyboardCollectionConfig = GetKeyboardConfigs();

            _config = keyboardCollectionConfig.Keyboards.SingleOrDefault( x => x.Keyboard == _kb.Name );
            if( _config == null ) _config = new SimpleSkin.ProcessBoundKeyboardConfig( _kb.Name );

            // _programGroup can be null, when eg. closing or during base constructor.
            if( _programGroup != null )
            {
                _programGroup.Items.Clear();
                if( _config.BoundProcessNames.Count == 0 )
                {
                    _programGroup.Items.Add( new TextItem( _app.ConfigManager, R.NoProgramsLinked, 12 ) );
                }
                else
                {
                    foreach( string processName in _config.BoundProcessNames )
                    {
                        _programGroup.Items.Add( GenerateProcessConfigItem( processName, () => { RemoveProcess( processName ); } ) );
                    }
                }
            }

            UpdateRadioGroupStatus();
        }

        void SaveConfig()
        {
            SimpleSkin.ProcessBoundKeyboardCollectionConfig configs = GetKeyboardConfigs( true );

            // Remove existing keyboard before adding it again
            var existingConfig = configs.Keyboards.SingleOrDefault( x => x.Keyboard == _kb.Name );
            if( existingConfig != null )
            {
                configs.Keyboards.Remove( existingConfig );
            }

            configs.Keyboards.Add( _config );

            if( Config != null )
            {
                var cs = Config.Set( "ProcessBoundKeyboardCollectionConfig", configs );
            }
        }

        void RemoveProcess( string processName )
        {
            _config.BoundProcessNames.Remove( processName );
            SaveConfig();
        }

        /// <summary>
        /// Gets the keyboard configs from the configuration. If needed, creates a new config object if it doesn't exist (or Config can't be used, like when plugin is stopped).
        /// </summary>
        /// <param name="mustReturnCopy">If true, will ALWAYS return a new object as a collection copy, so that it can be set afterwards.</param>
        /// <returns>Keyboard collection configuration.</returns>
        SimpleSkin.ProcessBoundKeyboardCollectionConfig GetKeyboardConfigs( bool mustReturnCopy = false )
        {
            SimpleSkin.ProcessBoundKeyboardCollectionConfig configs = new SimpleSkin.ProcessBoundKeyboardCollectionConfig();

            if( Config != null )
            {
                var savedConfigs = Config.GetOrSet( "ProcessBoundKeyboardCollectionConfig", configs );

                if( mustReturnCopy == true )
                {
                    configs.Keyboards.AddRange( savedConfigs.Keyboards );
                }
                else
                {
                    configs = savedConfigs;
                }
            }

            return configs;
        }

        string SelectProcess()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();

            ofd.Title = R.AddProgram;
            ofd.Filter = "Executables|*.exe";
            ofd.FilterIndex = 0;
            ofd.Multiselect = false;

            bool? result = ofd.ShowDialog();

            if( result == true )
            {
                return Path.GetFileNameWithoutExtension( ofd.FileName );
            }
            else
            {
                return null;
            }
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( _kb != null ) LoadConfig();
        }
    }
}
