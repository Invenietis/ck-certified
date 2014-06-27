using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            description.Items.Add( new TextItem( _app.ConfigManager, "\t- Alcatel-Lucent" ) );

            var combo = new ComboBoxItem( _app.ConfigManager, "Le clavier", _app.KeyboardContext.Keyboards.Select( k => k.Name ) );
            combo.SelectedItem = _app.CivikeyHost.UserConfig.GetOrSet( "PointerManager_KeyboardName", "Clavier-souris" );
            combo.SelectedItemChanged += combo_SelectedItemChanged;
            description.Items.Add( combo );
            
            base.OnInitialize();
        }

        void combo_SelectedItemChanged( object sender, SelectedItemChangedEventArgs e )
        {
            _app.CivikeyHost.UserConfig.Set( "PointerManager_KeyboardName", e.Item );
        }
    }
}
