#region LGPL License
/*----------------------------------------------------------------------------
* This file (CiviKey\Config\CVKTab.cs) is part of CiviKey. 
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
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using CK.Kernel.Plugin;
using CK.Application.Config;
using CK.Application;

namespace CK.Config.UI
{
	public partial class CVKTab : CVKTabBase
	{
		private bool _isClicked;

		public CVKTab( ICategory category )
			: base()
		{
			InitializeComponent();

			this.TabIcon.MouseEnter += new EventHandler( CVKTabMouseEnter );
			this.TabIcon.MouseLeave += new EventHandler( CVKTabMouseLeave );
			this.TabName.MouseEnter += new EventHandler( CVKTabMouseEnter );
			this.TabName.MouseLeave += new EventHandler( CVKTabMouseLeave );
			this.TabName.Click += new EventHandler( CVKTabClick );
			this.TabIcon.Click += new EventHandler( CVKTabClick );
			this.MouseEnter += new EventHandler( CVKTab_MouseEnter );
			this.MouseLeave += new EventHandler( CVKTab_MouseLeave );

			this.Tag = category;
			this.TabName.Text = String.IsNullOrEmpty( category.Title ) ? category.Name : category.Title;
            //if( category.Icon != null )
            //{
            //    this.TabIcon.Image = category.Icon.ToBitmap();
            //}
            //else
            //{
                this.TabIcon.Image = Resources.logo_cvk_32x32;
            //}
			this.Dock = DockStyle.Left;
		}

		public void AddTab( PluginStub Stub, String CatergoryName, Bitmap CategoryIcon )
		{
			this.TabIcon.Image = CategoryIcon;
			this.TabIcon.Tag = Stub;
			this.TabName.Text = CatergoryName;
			this.TabName.Tag = Stub;
		}
		void CVKTabClick( object sender, EventArgs e )
		{
			OnClick( e );
		}
		void CVKTabMouseLeave( object sender, EventArgs e )
		{
			OnMouseLeave( e );
		}
		void CVKTabMouseEnter( object sender, EventArgs e )
		{
			OnMouseEnter( e );
		}

		void CVKTab_MouseEnter( object sender, EventArgs e )
		{
			((CVKTab)sender).BackColor = Color.LightBlue;
		}

		void CVKTab_MouseLeave( object sender, EventArgs e )
		{
			((CVKTab)sender).BackColor = Color.Transparent;
		}

		public bool IsClicked
		{
			get { return _isClicked; }
			set 
			{ 
				_isClicked = value;
				this.BackgroundImage = _isClicked ? Resources.DegradeBlue : null;
				Invalidate();
			}
		}
	}
}
