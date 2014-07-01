using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    public class KeyboardPointerSelector : ConfigPage
    {
        AppViewModel _app;

        public KeyboardPointerSelector( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.Credits;
            _app = app;
        }

        protected override void OnInitialize()
        {
            var description = this.AddGroup();
            description.Items.Add( new TitleItem( _app.ConfigManager, "Selection du clavier" ) );
            description.Items.Add( new DescriptionItem( _app.ConfigManager, "La description" ) );

            var combo = new ComboBoxItem( _app.ConfigManager, "Le clavier", _app.KeyboardContext.Keyboards.Select( k => k.Name ) );

            string selectedKeyboard = _app.CivikeyHost.UserConfig.GetOrSet( "PointerManager_KeyboardName", "Clavier-souris" );

            if( _app.KeyboardContext.Keyboards.FirstOrDefault( k => k.Name == selectedKeyboard ) != null )
                combo.SelectedItem = _app.CivikeyHost.UserConfig.GetOrSet( "PointerManager_KeyboardName", "Clavier-souris" );

            combo.SelectedItemChanged += combo_SelectedItemChanged;
            description.Items.Add( combo );
            
            base.OnInitialize();
        }

        void combo_SelectedItemChanged( object sender, SelectedItemChangedEventArgs e )
        {
            IKeyboard kb = _app.KeyboardContext.Keyboards.FirstOrDefault( k => k.Name == e.PreviousItem );
            if( kb != null && kb.IsActive )
            {
                if( kb != _app.KeyboardContext.CurrentKeyboard ) kb.IsActive = false;
                _app.KeyboardContext.Keyboards.Single( k => k.Name == e.Item ).IsActive = true;
            }
            _app.CivikeyHost.UserConfig.Set( "PointerManager_KeyboardName", e.Item );
        }
    }
}
