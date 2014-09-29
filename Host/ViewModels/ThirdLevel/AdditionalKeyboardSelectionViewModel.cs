using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows.Config;
using CK.WPF.ViewModel;
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
            CreateKeyboardList();
            _app.KeyboardContext.Keyboards.KeyboardCreated += Keyboards_KeyboardCreated;
            _app.KeyboardContext.Keyboards.KeyboardDestroyed += Keyboards_KeyboardDestroyed;
            _app.KeyboardContext.CurrentKeyboardChanged += KeyboardContext_CurrentKeyboardChanged;

            _app.KeyboardContext.Keyboards.KeyboardActivated += Keyboards_KeyboardActivated;
            _app.KeyboardContext.Keyboards.KeyboardDeactivated += Keyboards_KeyboardDeactivated;
        }

        void Keyboards_KeyboardDeactivated( object sender, KeyboardEventArgs e )
        {
            ConfigImplementationSelectorItem item;

            if( _keyboardItems.TryGetValue( e.Keyboard, out item ) )
            {
                item.IsSelected = false;
            }
        }

        void Keyboards_KeyboardActivated( object sender, KeyboardEventArgs e )
        {
            ConfigImplementationSelectorItem item;

            if( _keyboardItems.TryGetValue( e.Keyboard, out item ) )
            {
                item.IsSelected = true;
            }
        }

        void KeyboardContext_CurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            ConfigImplementationSelectorItem item;

            if( _keyboardItems.TryGetValue( e.Current, out item ) )
            {
                item.Description = "Main keyboard"; // TODO
                item.IsSelected = true;
                item.Enabled = false;
            }
            if( _keyboardItems.TryGetValue( e.Previous, out item ) )
            {
                item.Description = null;
                item.IsSelected = e.Previous.IsActive;
                item.Enabled = true;
            }
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
                CreateKeyboardProperty( kb );
            }
        }

        void CreateKeyboardProperty( IKeyboard kb )
        {
            ConfigPage page = new AdditionalKeyboardConfigurationViewModel( _app, kb );

            ConfigImplementationSelectorItem selectorItem = new ConfigImplementationSelectorItem(
                _app.ConfigManager,
                _emptyPluginCluster,
                page,
                String.Empty // Kept empty to enable checkbox
               );
            selectorItem.SelectAction = () => { kb.IsActive = true; };
            selectorItem.DeselectAction = () => { kb.IsActive = false; };
            selectorItem.IsSelected = kb.IsActive;

            selectorItem.DisplayName = kb.Name;
            selectorItem.Description = String.Empty;

            if( _app.KeyboardContext.CurrentKeyboard == kb )
            {
                selectorItem.Description = "Main keyboard"; // TODO
                selectorItem.IsSelected = true;
                selectorItem.Enabled = false;
            }

            Items.Add( selectorItem );
            _keyboardItems.Add( kb, selectorItem );
        }
        void DestroyKeyboardProperty( IKeyboard kb )
        {
            ConfigImplementationSelectorItem item;

            if( _keyboardItems.TryGetValue( kb, out item ) )
            {
                _keyboardItems.Remove( kb );
                Items.Remove( item );
            }
        }
    }
}
