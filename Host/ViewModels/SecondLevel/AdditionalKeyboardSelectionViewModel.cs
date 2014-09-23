using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    /// <summary>
    /// Additional keyboard selection config view. Called from RootConfigViewModel.
    /// </summary>
    class AdditionalKeyboardSelectionViewModel : ConfigPage
    {
        AppViewModel _app;
        Dictionary<IKeyboard,ConfigItemProperty<bool>> _keyboardProperties;
        ConfigGroup _keyboardGroup;

        public AdditionalKeyboardSelectionViewModel( AppViewModel _app )
            : base( _app.ConfigManager )
        {
            DisplayName = R.AdditionalKeyboardsDisplayName;
            this._app = _app;
            _keyboardProperties = new Dictionary<IKeyboard, ConfigItemProperty<bool>>();
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
            ConfigItemProperty<bool> kbProperty = _keyboardGroup.AddProperty( kb.Name, kb, x => x.IsActive );
            _keyboardProperties.Add( kb, kbProperty );
        }
        void DestroyKeyboardProperty( IKeyboard kb )
        {
            ConfigItemProperty<bool> property;

            if( _keyboardProperties.TryGetValue( kb, out property ) )
            {
                _keyboardProperties.Remove( kb );
                _keyboardGroup.Items.Remove( property );
            }
        }
    }
}
