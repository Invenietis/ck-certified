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
            partner.Items.Add( new TextItem( _app.ConfigManager, "   - Alcatel-Lucent" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "   - Fondation Garches" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "   - Plate-forme Nouvelles Technologies (PFNT), " ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "   à l'Hôpital Raymond Poincaré, Garches" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "   - INTECH INFO" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "   - Fondation Stéria" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "   - Invenietis" ) );
            partner.Items.Add( new TextItem( _app.ConfigManager, "" ) );

            var ergo = this.AddGroup();
            ergo.Items.Add( new TitleItem( _app.ConfigManager, "Ergothérapeutes (PFNT) :" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "   - Samuel Pouplin" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "   - Salvador Cabanilles" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "   - Justine Bouteille" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "   - De nombreux stagiaires" ) );
            ergo.Items.Add( new TextItem( _app.ConfigManager, "" ) );

            var dev = this.AddGroup();
            dev.Items.Add( new TitleItem( _app.ConfigManager, "Développeurs :" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "   - Olivier Spinelli" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "   - Guillaume Fradet" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "   - Cédric Legendre" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "   - Antoine Blanchet" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "   - Idriss Hippocrate" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "   - Isaac Duplan" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "   - Franck Bontemps" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "   - Maxime Paquatte" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "   - Julien Mathon" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "   - Jean-Loup Kahloun" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "   - De nombreux élèves d'INTECH INFO" ) );
            dev.Items.Add( new TextItem( _app.ConfigManager, "" ) );

            base.OnInitialize();
        }
    }
}
