using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    public class CreditViewModel : ConfigPage
    {
        AppViewModel _app;

        public CreditViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.Credits;
            _app = app;
        }

        protected override void OnInitialize()
        {
            var partner = this.AddGroup();
            partner.Items.Add( new TitleItem( _app.ConfigManager, "Partenaires :" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "\t- Jean-Loup le BG" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "\t- Jean-Loup le BG" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "" ) );

            var dev = this.AddGroup();
            dev.Items.Add( new TitleItem( _app.ConfigManager, "Développeurs :" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "\t- Jean-Loup le BG" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "\t- Jean-Loup le BG" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "" ) );

            var ergo = this.AddGroup();
            ergo.Items.Add( new TitleItem( _app.ConfigManager, "Ergothérapeuthes :" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "\t- Jean-Loup le BG" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "\t- Jean-Loup le BG" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "" ) );

            base.OnInitialize();
        }
    }
}
