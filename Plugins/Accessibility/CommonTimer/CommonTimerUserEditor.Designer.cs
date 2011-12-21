namespace CK.Plugins.CommonTimer
{
	partial class CommonTimerUserEditor
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
            this.gbBox = new System.Windows.Forms.GroupBox();
            this.nInteval = new System.Windows.Forms.NumericUpDown();
            this.btSave = new System.Windows.Forms.Button();
            this.lInterval = new System.Windows.Forms.Label();
            this.gbBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nInteval)).BeginInit();
            this.SuspendLayout();
            // 
            // gbBox
            // 
            this.gbBox.Controls.Add( this.nInteval );
            this.gbBox.Controls.Add( this.btSave );
            this.gbBox.Controls.Add( this.lInterval );
            this.gbBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbBox.Location = new System.Drawing.Point( 0, 0 );
            this.gbBox.Margin = new System.Windows.Forms.Padding( 10 );
            this.gbBox.Name = "gbBox";
            this.gbBox.Size = new System.Drawing.Size( 335, 148 );
            this.gbBox.TabIndex = 0;
            this.gbBox.TabStop = false;
            this.gbBox.Text = "Configuration du Plugin Common Timer";
            // 
            // nInteval
            // 
            this.nInteval.Location = new System.Drawing.Point( 9, 55 );
            this.nInteval.Maximum = new decimal( new int[] {
            100000,
            0,
            0,
            0} );
            this.nInteval.Name = "nInteval";
            this.nInteval.Size = new System.Drawing.Size( 320, 20 );
            this.nInteval.TabIndex = 3;
            // -
            // btSave
            // 
            this.btSave.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btSave.Location = new System.Drawing.Point( 3, 122 );
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size( 329, 23 );
            this.btSave.TabIndex = 2;
            this.btSave.Text = "Sauver";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler( this.OnSave );
            // 
            // lInterval
            // 
            this.lInterval.AutoSize = true;
            this.lInterval.Location = new System.Drawing.Point( 6, 39 );
            this.lInterval.Name = "lInterval";
            this.lInterval.Size = new System.Drawing.Size( 162, 13 );
            this.lInterval.TabIndex = 0;
            this.lInterval.Text = "Interval du timer (millisecondes) : ";
            // 
            // CommonTimerUserEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.gbBox );
            this.Name = "CommonTimerUserEditor";
            this.Size = new System.Drawing.Size( 335, 148 );
            this.gbBox.ResumeLayout( false );
            this.gbBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nInteval)).EndInit();
            this.ResumeLayout( false );

		}

		#endregion

		private System.Windows.Forms.GroupBox gbBox;
		private System.Windows.Forms.Button btSave;
		private System.Windows.Forms.Label lInterval;
		private System.Windows.Forms.NumericUpDown nInteval;
	}
}
