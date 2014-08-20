#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\ThirdLevel\CreditViewModel.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
            partner.Items.Add( new TextItem( _app.ConfigManager, "   - Université de Tours" ) );
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
