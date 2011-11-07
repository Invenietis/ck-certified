#region LGPL License
/*----------------------------------------------------------------------------
* This file (CiviKey\Config\CVKConfigForm.cs) is part of CiviKey. 
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
using System.Collections;
using CK.Kernel.Plugin;
using System.Diagnostics;
using CK.Application.Config;
using CK.Plugin;
using CK.Plugin.Hosting;

namespace CK.Config.UI
{
    public partial class ConfigForm : Form
    {
        IPluginHost<IPlugin> _pluginHost;
		/// <summary>
		/// The list of <see cref="Panel"/> by <see cref="CVKCategory"/>.
		/// </summary>
		IDictionary<ICategory,Panel> _container;

        /// <summary>
		/// Initializes a new <see cref="ConfigForm"/> and keeps a reference to the <see cref="ICVKPluginManager"/>
        /// </summary>
		/// <param name="pluginHost"><see cref="ICVKPluginManager"/>.</param>
        public ConfigForm( IPluginHost<IPlugin> pluginHost)
        {
            InitializeComponent();
			_pluginHost = pluginHost;
			_container = new Dictionary<ICategory, Panel>();

			SetStyle( ControlStyles.OptimizedDoubleBuffer, true );
			SetStyle( ControlStyles.AllPaintingInWmPaint, true );
			this.Size = new System.Drawing.Size( 596, 501 );
			this.Icon = Resources.logo_cvk;
		}

		protected override void OnLoad( EventArgs e )
		{
			int tabIndex = -1;
			if( _pluginHost.LoadedPlugins != null )
			{
                //BOOKMARK : TODO
                //foreach( ICategory category in _pluginHost.CategoryManager.Categories )
                //{
                //    CVKTab tabCVK = new CVKTab( category );

                //    tabCVK.Click += new EventHandler( OnCVKTabClick );
                //    tabCVK.TabIndex = ++tabIndex;
                //    _mainPluginPanel.Tag = category;

                //    TableLayoutPanel p = _mainPluginPanel.CreateCategoryPanel( category );
                //    p.RowCount = category.Plugins.Count;
                //    foreach( IPluginLoaderInfo info in category.Plugins )
                //    {
                //        IPluginInfo pluginInfo = info.Info;
                //        CVKPluginPanel panel = new CVKPluginPanel(_pluginHost[pluginInfo.PluginId], _pluginHost);
                //        panel.Click += new EventHandler( PanelClick );
                //        p.Controls.Add( panel );
                //    }
                //    _cvkTabContainer.Controls.Add( tabCVK );

                //}

                int nbCategory = _cvkTabContainer.Controls.Count;
                if (nbCategory > 0)
                {
                    _currentTab = _cvkTabContainer.Controls[nbCategory - 1] as CVKTab;
                    _currentTab.IsClicked = true;
                    _mainPluginPanel.ShowCategoryPanel(_currentTab.Tag as ICategory);
                }

			}
		}

		CVKPluginPanel _currentPluginPanel;
		CVKPluginPanel _oldPluginPanel;

		/// <summary>
		/// Hihglights the selected <see cref="CVKPluginPanel"/>, and display all needed button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void PanelClick( object sender, EventArgs e )
		{
			this.SuspendLayout();
			CVKPluginPanel selectedPanel = sender as CVKPluginPanel;
			SelectPluginPanel( selectedPanel );
			this.ResumeLayout( false );
			this.PerformLayout();
		}

		private void SelectPluginPanel( CVKPluginPanel selectedPanel )
		{

			_currentPluginPanel = selectedPanel;

			if( _oldPluginPanel != _currentPluginPanel )
			{
				if( _oldPluginPanel != null && _oldPluginPanel.IsSelected )
				{
					_oldPluginPanel.IsSelected = false;
				}

				if( !selectedPanel.IsSelected )
				{
					selectedPanel.IsSelected = true;
				}

				_oldPluginPanel = _currentPluginPanel;
			}
			selectedPanel.Parent.Focus();
		}
		
		CVKTab _currentTab;
		CVKTab _oldTab;

		/// <summary>
		/// Shows all the <see cref="CVKPluginPanel"/> tagged by the <see cref="CVKCategory"/> selected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        void OnCVKTabClick(object sender, EventArgs e)
        {
			if( _currentTab != null && _currentTab != _oldTab )
			{
				_oldTab = _currentTab;
				_oldTab.IsClicked = false;
			}
			_currentTab = (CVKTab)(sender);
			_currentTab.IsClicked = true;

			_mainPluginPanel.ShowCategoryPanel( _currentTab.Tag as ICategory );
        }

    }
}