#region LGPL License
/*----------------------------------------------------------------------------
* This file (CiviKey\Config\CVKPluginPanel.cs) is part of CiviKey. 
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
using System.Drawing.Drawing2D;
using CK.Application.Config;
using CK.Application;
using CK.Core;
using CK.Plugin;

namespace CK.Config.UI
{

	public partial class CVKPluginPanel : UserControl
	{
		readonly PluginStub _pluginStub;
		readonly IPluginManager _pluginManager;
		bool _isSelected;
		private bool _startStopPluginComponents;
		private bool _hasEditor;

		class CVKStatusListItem
		{
			public CVKStatusListItem( string text, ConfigPluginStatus value )
			{
				Text = text;
				Value = value;
			}
			/// <summary>
			/// The value associates to this item
			/// </summary>
			public ConfigPluginStatus Value { get; set; }
			/// <summary>
			/// The text to display in the combobox
			/// </summary>
			public string Text { get; set; }
		}

		public PluginStub PluginStub
		{
			get { return _pluginStub; }
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;
				StartStopPluginManagement = _isSelected;
				this.Refresh();
			}
		}
		/// <summary>
		/// Gets or Sets wether the control displays the management 
		/// of start or stop a plugin.
		/// </summary>
		public bool StartStopPluginManagement
		{
			get { return _startStopPluginComponents; }
			set
			{
				_startStopPluginComponents = value;

				if( _isSelected )
				{
					this.BackgroundImage = Resources.DegradeBlue;
				}
				else
				{
					this.BackgroundImage = null;
				}
				pluginStartStopButton.Visible = _startStopPluginComponents;
				pluginStartTypeCombo.Visible = _startStopPluginComponents;
				pluginOptions.Visible = _startStopPluginComponents;
				pluginOptions.Enabled = _hasEditor && _pluginStub.Status != ConfigPluginStatus.Disabled;
			}
		}
		/// <summary>
        /// Get or Set the value indicating the start option of the selected plugin.
		/// </summary>
		public PluginStatus StartType { get; set; }

		public CVKPluginPanel( PluginStub stub, IPluginManager pluginManager )
		{
			_pluginStub = stub;
			_pluginManager = pluginManager;
			_hasEditor = _pluginManager.GetEditorPlugins( ConfigTypeDescriptor.User, PluginStub.LoaderInfo.Info.PluginId ).Count > 0;
			InitializeComponent();
			InitPluginCombobox();
			pluginOptions.Enabled = pluginStartStopButton.Enabled =  PluginStub.Status != ConfigPluginStatus.Disabled;
			pluginInfoTooltip.SetToolTip( pluginStartStopButton, PluginStub.Live != null ? PluginStub.Live.LastError : "" );
			pluginInfoTooltip.SetToolTip( pluginIcon, PluginStub.Live != null ? PluginStub.Live.LastError : "" );

			pluginInfoTooltip.Popup += new PopupEventHandler( pluginInfoTooltip_Popup );
			pluginExtensionDescription.Text = PluginStub.LoaderInfo.Info.Description;
			pluginExtensionName.Text = PluginStub.LoaderInfo.Info.PublicName;
            pluginVersion.Text = PluginStub.LoaderInfo.Info.Version.ToString();

            //if( PluginStub.LoaderInfo.Info.IconUri != null )
            //{
            //    Icon icon = new Icon( PluginStub.LoaderInfo.Info.IconUri.ToString() );
            //    pluginIcon.Size = icon.Size;
            //    pluginIcon.Image = icon.ToBitmap();
            //}
            //else
            //{
				pluginIcon.Size = Resources.logo_cvk_48x48.Size;
				pluginIcon.Image = Resources.logo_cvk_48x48;
            //}
			this.Height = 61;
			IsSelected = false;
			Dock = DockStyle.Top;
			
			if( PluginStub.Live != null )
			{
				pluginStartStopButton.Text = PluginStub.Live.IsRunning ? Resources.stopP : Resources.startP;
			}
			else
			{
				pluginStartStopButton.Text = Resources.startP;
			}


			stub.Started += new EventHandler( OnStubStarted );
			stub.Stoped += new EventHandler( OnStubStoped );

			pluginStartStopButton.Click += new EventHandler( OnPluginStartStopClick );
			pluginOptions.Click += new EventHandler( OnOptionsClick );
			pluginStartTypeCombo.SelectedIndexChanged += new EventHandler( OnPluginRunComboSelectedIndexChanged );
			pluginExtensionDescription.Click += new EventHandler( PanelClick );
			pluginExtensionName.Click += new EventHandler( PanelClick );
			pluginIcon.Click += new EventHandler( PanelClick );

			this.SetStyle( ControlStyles.AllPaintingInWmPaint, true );
			this.SetStyle( ControlStyles.OptimizedDoubleBuffer, true );
		}

		void pluginInfoTooltip_Popup( object sender, PopupEventArgs e )
		{
		}

		private void InitPluginCombobox()
		{
			CVKStatusListItem[] statusList = new CVKStatusListItem[] {
				new CVKStatusListItem( Resources.disable, ConfigPluginStatus.Disabled  ),
				new CVKStatusListItem( Resources.automatic, ConfigPluginStatus.AutomaticStart ),
				new CVKStatusListItem( Resources.manual, ConfigPluginStatus.Manual )
			}; 
			pluginStartTypeCombo.DataSource = statusList;
			pluginStartTypeCombo.DisplayMember = "Text";
			pluginStartTypeCombo.ValueMember = "Value";
			
			pluginStartTypeCombo.SelectedValue = PluginStub.Status;
		}

		void PanelClick( object sender, EventArgs e )
		{
			this.OnClick( e );
		}

		void OnStubStoped( object sender, EventArgs e )
		{
			pluginStartStopButton.Text = Resources.startP;
		}

		void OnStubStarted( object sender, EventArgs e )
		{
			pluginStartStopButton.Text = Resources.stopP;
		}

		/// <summary>
		/// Options editor panel.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOptionsClick( object sender, EventArgs e )
		{
			IReadOnlyList<PluginStub> editors = _pluginManager.GetEditorPlugins( ConfigTypeDescriptor.User, PluginStub.LoaderInfo.Info.PluginId );
			if( editors.Count> 0 )
			{
				CVKEditorPanel editorPanel = new CVKEditorPanel();
				editorPanel.FillEditorList( editors );
				editorPanel.Dock = DockStyle.Fill;

				using( Form editorForm = new Form() )
				{
					editorForm.Text = _pluginStub.LoaderInfo.Info.PublicName;
					editorForm.Padding = new Padding( 10, 10, 5, 15 );
					editorForm.Controls.Add( editorPanel );
					editorForm.ClientSize = editorPanel.Size;
					editorForm.ShowDialog();
				}
			}
		}

		/// <summary>
		/// Start or Stop a plugin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPluginStartStopClick( object sender, EventArgs e )
		{
			if( PluginStub.Live != null )
			{
				if( PluginStub.Live.IsRunning ) PluginStub.Live.Stop();
				else
				{
					if( !PluginStub.Live.Start() )
					{
						Debug.Assert( PluginStub.Live.LastError != null );
						//pluginInfoTooltip.Show( PluginStub.Live.LastError, pluginExtensionName, 6000 );
					}
				}
			}
		}

		/// <summary>
		/// ComboBox change. Disable / AutoStart / Manual.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPluginRunComboSelectedIndexChanged( object sender, EventArgs e )
		{
			CVKStatusListItem selectedItem = (CVKStatusListItem)pluginStartTypeCombo.SelectedItem;
			pluginOptions.Enabled = pluginStartStopButton.Enabled =  selectedItem.Value != ConfigPluginStatus.Disabled;
			
			PluginStub.Status = selectedItem.Value;

			if( !pluginOptions.Visible && _hasEditor ) pluginOptions.Show();
		}
	}
}
