namespace CK.Config.UI
{
	partial class CVKPluginPanel
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.pluginStartStopButton = new System.Windows.Forms.Button();
            this.pluginStartTypeCombo = new System.Windows.Forms.ComboBox();
            this.pluginExtensionName = new System.Windows.Forms.Label();
            this.pluginIcon = new System.Windows.Forms.PictureBox();
            this.pluginExtensionDescription = new System.Windows.Forms.Label();
            this.pluginVersion = new System.Windows.Forms.Label();
            this.pluginInfoTooltip = new System.Windows.Forms.ToolTip( this.components );
            this.pluginOptions = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pluginIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // pluginStartStopButton
            // 
            this.pluginStartStopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pluginStartStopButton.Location = new System.Drawing.Point( 84, 65 );
            this.pluginStartStopButton.Name = "pluginStartStopButton";
            this.pluginStartStopButton.Size = new System.Drawing.Size( 75, 23 );
            this.pluginStartStopButton.TabIndex = 1;
            this.pluginStartStopButton.Text = "Start";
            // 
            // pluginStartTypeCombo
            // 
            this.pluginStartTypeCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pluginStartTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.pluginStartTypeCombo.FormattingEnabled = true;
            this.pluginStartTypeCombo.Location = new System.Drawing.Point( 165, 67 );
            this.pluginStartTypeCombo.Name = "pluginStartTypeCombo";
            this.pluginStartTypeCombo.Size = new System.Drawing.Size( 179, 21 );
            this.pluginStartTypeCombo.TabIndex = 2;
            // 
            // pluginExtensionName
            // 
            this.pluginExtensionName.AutoSize = true;
            this.pluginExtensionName.BackColor = System.Drawing.Color.Transparent;
            this.pluginExtensionName.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)) );
            this.pluginExtensionName.Location = new System.Drawing.Point( 81, 4 );
            this.pluginExtensionName.Name = "pluginExtensionName";
            this.pluginExtensionName.Size = new System.Drawing.Size( 41, 13 );
            this.pluginExtensionName.TabIndex = 3;
            this.pluginExtensionName.Text = "label2";
            // 
            // pluginIcon
            // 
            this.pluginIcon.BackColor = System.Drawing.Color.Transparent;
            this.pluginIcon.Location = new System.Drawing.Point( 4, 4 );
            this.pluginIcon.Name = "pluginIcon";
            this.pluginIcon.Size = new System.Drawing.Size( 74, 52 );
            this.pluginIcon.TabIndex = 18;
            this.pluginIcon.TabStop = false;
            // 
            // pluginExtensionDescription
            // 
            this.pluginExtensionDescription.AutoEllipsis = true;
            this.pluginExtensionDescription.BackColor = System.Drawing.Color.Transparent;
            this.pluginExtensionDescription.Location = new System.Drawing.Point( 81, 26 );
            this.pluginExtensionDescription.Name = "pluginExtensionDescription";
            this.pluginExtensionDescription.Size = new System.Drawing.Size( 395, 30 );
            this.pluginExtensionDescription.TabIndex = 5;
            this.pluginExtensionDescription.Text = "label1";
            // 
            // pluginVersion
            // 
            this.pluginVersion.AutoSize = true;
            this.pluginVersion.BackColor = System.Drawing.Color.Transparent;
            this.pluginVersion.Location = new System.Drawing.Point( 441, 4 );
            this.pluginVersion.Name = "pluginVersion";
            this.pluginVersion.Size = new System.Drawing.Size( 35, 13 );
            this.pluginVersion.TabIndex = 4;
            this.pluginVersion.Text = "label1";
            // 
            // pluginInfoTooltip
            // 
            this.pluginInfoTooltip.AutomaticDelay = 200;
            this.pluginInfoTooltip.AutoPopDelay = 6000;
            this.pluginInfoTooltip.InitialDelay = 200;
            this.pluginInfoTooltip.IsBalloon = true;
            this.pluginInfoTooltip.ReshowDelay = 40;
            this.pluginInfoTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Warning;
            // 
            // pluginOptions
            // 
            this.pluginOptions.Location = new System.Drawing.Point( 3, 65 );
            this.pluginOptions.Name = "pluginOptions";
            this.pluginOptions.Size = new System.Drawing.Size( 75, 23 );
            this.pluginOptions.TabIndex = 19;
            this.pluginOptions.Text = "Options";
            // 
            // CVKPluginPanel
            // 
            this.AutoSize = true;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add( this.pluginOptions );
            this.Controls.Add( this.pluginVersion );
            this.Controls.Add( this.pluginExtensionDescription );
            this.Controls.Add( this.pluginIcon );
            this.Controls.Add( this.pluginExtensionName );
            this.Controls.Add( this.pluginStartStopButton );
            this.Controls.Add( this.pluginStartTypeCombo );
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size( 300, 40 );
            this.Name = "CVKPluginPanel";
            this.Size = new System.Drawing.Size( 479, 91 );
            ((System.ComponentModel.ISupportInitialize)(this.pluginIcon)).EndInit();
            this.ResumeLayout( false );
            this.PerformLayout();

		}

		#endregion

		protected System.Windows.Forms.Button pluginStartStopButton;
		protected System.Windows.Forms.ComboBox pluginStartTypeCombo;
		private System.Windows.Forms.Label pluginExtensionName;
		private System.Windows.Forms.PictureBox pluginIcon;
		private System.Windows.Forms.Label pluginExtensionDescription;
		private System.Windows.Forms.Label pluginVersion;
		private System.Windows.Forms.ToolTip pluginInfoTooltip;
		private System.Windows.Forms.Button pluginOptions;

	}
}
