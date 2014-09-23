using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    /// <summary>
    /// Additional keyboard selection config view. Called from KeyboardConfigViewModel.
    /// </summary>
    class AdditionalKeyboardSelectionViewModel : ConfigPage
    {
        readonly AppViewModel _app;
        readonly ISimplePluginRunner _runner;
        readonly IUserConfiguration _userConf;
        readonly Dictionary<IKeyboard,ConfigImplementationSelectorItem> _keyboardItems;
        readonly PluginCluster _emptyPluginCluster;

        ConfigGroup _keyboardGroup;

        public AdditionalKeyboardSelectionViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.AdditionalKeyboardsDisplayName;

            _app = app;
            _runner = _app.PluginRunner;
            _userConf = _app.CivikeyHost.Context.ConfigManager.UserConfiguration;

            _keyboardItems = new Dictionary<IKeyboard, ConfigImplementationSelectorItem>();
            _emptyPluginCluster = new PluginCluster( _runner, _userConf, Guid.Empty );
        }

        protected override void OnInitialize()
        {
            _keyboardGroup = this.AddGroup();

            CreateKeyboardList();
            _app.KeyboardContext.Keyboards.KeyboardCreated += Keyboards_KeyboardCreated;
            _app.KeyboardContext.Keyboards.KeyboardDestroyed += Keyboards_KeyboardDestroyed;
            _app.KeyboardContext.CurrentKeyboardChanged += KeyboardContext_CurrentKeyboardChanged;
        }

        void KeyboardContext_CurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            DestroyKeyboardProperty( e.Current );
            CreateKeyboardProperty( e.Previous );
        }

        void Keyboards_KeyboardDestroyed( object sender, KeyboardEventArgs e )
        {
            DestroyKeyboardProperty( e.Keyboard );
        }

        void Keyboards_KeyboardCreated( object sender, KeyboardEventArgs e )
        {
            CreateKeyboardProperty( e.Keyboard );
        }

        void CreateKeyboardList()
        {
            foreach( IKeyboard kb in _app.KeyboardContext.Keyboards )
            {
                // Do not add the Current (main) keyboard to the list
                if( _app.KeyboardContext.CurrentKeyboard != kb )
                {
                    CreateKeyboardProperty( kb );
                }
            }
        }

        void CreateKeyboardProperty( IKeyboard kb )
        {
            ConfigImplementationSelectorItem selectorItem = new ConfigImplementationSelectorItem(
                _app.ConfigManager,
                _emptyPluginCluster,
                String.Empty // Kept empty to enable checkbox
               );
            selectorItem.SelectAction = () => { kb.IsActive = true; };
            selectorItem.DeselectAction = () => { kb.IsActive = false; };

            selectorItem.DisplayName = kb.Name;
            selectorItem.Description = String.Empty;

            Items.Add( selectorItem );
            _keyboardItems.Add( kb, selectorItem );
        }
        void DestroyKeyboardProperty( IKeyboard kb )
        {
            ConfigImplementationSelectorItem item;

            if( _keyboardItems.TryGetValue( kb, out item ) )
            {
                _keyboardItems.Remove( kb );
                _keyboardGroup.Items.Remove( item );
            }
        }
    }
}
