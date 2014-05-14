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
            partner.Items.Add( new TextItem( _app.ConfigManager, "\t- Alcatel-Lucent" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "\t- Fondation Garches" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "\t- PFNT" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "\t- INTECH INFO" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "\t- Fondation Stéria" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "\t- Invenietis" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "" ) );

            var ergo = this.AddGroup();
            ergo.Items.Add( new TitleItem( _app.ConfigManager, "Ergothérapeuthes (PFNT) :" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "\t- Samuel Pouplin" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "\t- Salvador Cabanilles" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "\t- Justine Bouteille" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "\t- Nicolas Auric" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "\t- Célie Chanéac" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "" ) );

            var dev = this.AddGroup();
            dev.Items.Add( new TitleItem( _app.ConfigManager, "Développeurs :" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "\t- Olivier Spinelli" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "\t- Guillaume Fradet" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "\t- Cédric Legendre" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "\t- Antoine Blanchet" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "\t- Idriss Hippocrate" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "\t- Isaac Duplan" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "\t- Franck Bontemps" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "\t- Maxime Paquatte" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "\t- Julien Mathon" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "\t- Jean-Loup Kahloun" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "\t- De nombreux élèves d'INTECH INFO" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "" ) );

            base.OnInitialize();
        }
    }
}
