#region LGPL License
/*----------------------------------------------------------------------------
* This file (CiviKey\Config\CVKMainPlugin.cs) is part of CiviKey. 
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
* Copyright © 2007-2009, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CK.Kernel.Plugin;

namespace CK.Config.UI
{
    public partial class CVKMainPlugin : Panel
    {
		IDictionary<ICategory,Panel> _container;

		internal Panel GetContainerPanel( ICategory c )
		{
			return _container[c];
		}

        public CVKMainPlugin()
        {
            InitializeComponent();
			_container = new Dictionary<ICategory, Panel>();
        }

		internal TableLayoutPanel CreateCategoryPanel( ICategory category )
		{
			TableLayoutPanel categoryPluginsPanel = new TableLayoutPanel()
			{
				Name = category.Name,
				Tag = category,
				AutoScroll = true,
				AutoSize = true,
				Visible = false
			};
			categoryPluginsPanel.VerticalScroll.Enabled = true;
			_container.Add( new KeyValuePair<ICategory, Panel>( category, categoryPluginsPanel ) );
			return categoryPluginsPanel;
		}

		internal void ShowCategoryPanel( ICategory category )
		{
			if( this.Controls.Count > 0) this.Controls.RemoveAt( 0 );
			Panel p = _container[category];
			this.Controls.Add( p );
			p.Dock = DockStyle.Fill;
			p.Show();
		}
	}

    internal class PluginComparer : IComparer<PluginStub>
    {
        public int Compare(PluginStub x, PluginStub y)
        {
            return y.LoaderInfo.Info.PublicName.CompareTo(x.LoaderInfo.Info.PublicName);
        }
    }
}