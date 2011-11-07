namespace CK.Application.Config
{
	partial class CVKEditorPanel
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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.editorList = new System.Windows.Forms.ListView();
			this.editorDisplayer = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Location = new System.Drawing.Point( 0, 0 );
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add( this.label1 );
			this.splitContainer1.Panel1.Controls.Add( this.editorList );
			this.splitContainer1.Panel1MinSize = 170;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add( this.editorDisplayer );
			this.splitContainer1.Size = new System.Drawing.Size( 749, 439 );
			this.splitContainer1.SplitterDistance = 195;
			this.splitContainer1.TabIndex = 0;
			// 
			// editorList
			// 
			this.editorList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.editorList.Location = new System.Drawing.Point( 1, 16 );
			this.editorList.MultiSelect = false;
			this.editorList.Name = "editorList";
			this.editorList.Size = new System.Drawing.Size( 192, 420 );
			this.editorList.TabIndex = 1;
			this.editorList.UseCompatibleStateImageBehavior = false;
			this.editorList.View = System.Windows.Forms.View.Tile;
			this.editorList.ItemActivate += new System.EventHandler( this.OnItemActivate );
			// 
			// editorDisplayer
			// 
			this.editorDisplayer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.editorDisplayer.Location = new System.Drawing.Point( 0, 0 );
			this.editorDisplayer.Name = "editorDisplayer";
			this.editorDisplayer.Size = new System.Drawing.Size( 550, 439 );
			this.editorDisplayer.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point( 3, 0 );
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size( 89, 13 );
			this.label1.TabIndex = 0;
			this.label1.Text = "Liste des éditeurs";
			// 
			// CVKEditorPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add( this.splitContainer1 );
			this.Name = "CVKEditorPanel";
			this.Size = new System.Drawing.Size( 749, 439 );
			this.splitContainer1.Panel1.ResumeLayout( false );
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout( false );
			this.splitContainer1.ResumeLayout( false );
			this.ResumeLayout( false );

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ListView editorList;
		private System.Windows.Forms.Panel editorDisplayer;
		private System.Windows.Forms.Label label1;
	}
}
