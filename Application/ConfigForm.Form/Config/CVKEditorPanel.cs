#region LGPL License
/*----------------------------------------------------------------------------
* This file (CiviKey\Config\CVKEditorPanel.cs) is part of CiviKey. 
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
using System.Diagnostics;
using CK.Plugin;
using CK.Plugin.Hosting;

namespace CK.Application.Config
{
	public partial class CVKEditorPanel : UserControl
	{
		public CVKEditorPanel()
		{
			InitializeComponent();
		}

        public void FillEditorList( IEnumerable<IPluginProxy<IPlugin>> editorStubList )
		{
			ImageList smallImageList = new ImageList();
			foreach( PluginStub editor in editorStubList )
			{
                Icon icon = null;
                if( editor.LoaderInfo.Info.IconUri != null )
                {
                    icon = new Icon( editor.LoaderInfo.Info.IconUri.AbsolutePath );
                }

                Bitmap currentEditorImage = icon != null ? icon.ToBitmap() : Resources.Run;
				smallImageList.Images.Add( editor.LoaderInfo.Info.PublicName, currentEditorImage );
				ListViewItem editorItem = new ListViewItem()
				{
					Name = editor.LoaderInfo.Info.PublicName,
					Tag = editor,
					Text = editor.LoaderInfo.Info.PublicName,
					ImageKey = editor.LoaderInfo.Info.PublicName,
					
				};
				
				editorList.Items.Add( editorItem );
					
			}
			editorList.LargeImageList = smallImageList;
            if (editorList.Items.Count == 1)
                splitContainer1.Panel1Collapsed = true;
            
		}
		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );
			if( editorList.Items.Count > 0 )
			{
				PluginStub editorStub =  editorList.Items[0].Tag as PluginStub;
				if( editorStub != null )
				{
					DisplayEditorPanel( editorStub );
				}
			}

		}

		private void OnItemActivate( object sender, EventArgs e )
		{
			if( editorList.MultiSelect )
				throw new ApplicationException( "MultiSelect not allowed" );

			PluginStub editorStub = editorList.SelectedItems[0].Tag as PluginStub;

			DisplayEditorPanel( editorStub );
		}

		private void DisplayEditorPanel( PluginStub editorStub )
		{
			Panel editorPanel = editorStub.Edition.CreatePanel( new string[]{"User"}, editorStub.Edition.EditedPluginConfig.User );

			if( editorDisplayer.Controls.Count > 0 ) editorDisplayer.Controls.RemoveAt( 0 );
			editorDisplayer.Controls.Add( editorPanel );
			editorDisplayer.Size = editorPanel.Size;
			FindForm().ClientSize = new Size( editorPanel.Width + (splitContainer1.Panel1Collapsed ? 0 : editorList.Width + 15), editorPanel.Height + 15);
			editorPanel.Dock = DockStyle.Fill;
		}
	}
}
