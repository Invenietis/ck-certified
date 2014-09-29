using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows.Config;
using CK.WPF.ViewModel;
using Host.Resources;

namespace Host.VM
{
    /// <summary>
    /// Additional keyboard configuration config view.
    /// </summary>
    class AdditionalKeyboardConfigurationViewModel : ConfigBase
    {
        readonly AppViewModel _app;
        readonly IUserConfiguration _userConf;
        readonly IKeyboard _kb;

        ConfigGroup _programGroup;
        SimpleSkin.ProcessBoundKeyboardConfig _config;

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
            _app = app;
            _userConf = _app.CivikeyHost.Context.ConfigManager.UserConfiguration;
        }

        protected override void OnInitialize()
        {
            this.Description = _kb.Name;

            _programGroup = this.AddGroup();

            this.AddAction( "Ajouter un programme", AddProcess );

            LoadConfig();
        }

        void AddProcess()
        {
            string processName = SelectProcess();

            if( processName != null && !_config.BoundProcessNames.Contains( processName ) )
            {
                _programGroup.Items.Add( new Label() { Content = processName } ); // TODO

                _config.BoundProcessNames.Add( processName );
                SaveConfig();
            }
        }

        void LoadConfig()
        {
            SimpleSkin.ProcessBoundKeyboardCollectionConfig configs = GetKeyboardConfigs();

            _config = configs.Keyboards.SingleOrDefault( x => x.Keyboard == _kb.Name );
            if( _config == null ) _config = new SimpleSkin.ProcessBoundKeyboardConfig( _kb.Name );

            // _programGroup can be null, when eg. closing.
            if( _programGroup != null )
            {
                _programGroup.Items.Clear();
                foreach( string processName in _config.BoundProcessNames )
                {
                    _programGroup.Items.Add( new Label() { Content = processName } ); // TODO
                }
            }
        }

        void SaveConfig()
        {
            SimpleSkin.ProcessBoundKeyboardCollectionConfig configs = GetKeyboardConfigs( true );

            // Remove existing keyboard
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

            ofd.Title = "Ajouter un programme"; // TODO
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
