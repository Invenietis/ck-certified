using System.Drawing;
namespace CK.Config.UI
{
    public partial class ConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( ConfigForm ) );
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this._cvkTabContainer = new System.Windows.Forms.Panel();
			this._mainPluginPanel = new CK.Config.UI.CVKMainPlugin();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			resources.ApplyResources( this.splitContainer1, "splitContainer1" );
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add( this._cvkTabContainer );
			// 
			// splitContainer1.Panel2
			// 
			resources.ApplyResources( this.splitContainer1.Panel2, "splitContainer1.Panel2" );
			this.splitContainer1.Panel2.Controls.Add( this._mainPluginPanel );
			// 
			// _cvkTabContainer
			// 
			resources.ApplyResources( this._cvkTabContainer, "_cvkTabContainer" );
			this._cvkTabContainer.BackColor = System.Drawing.Color.White;
			this._cvkTabContainer.Name = "_cvkTabContainer";
			// 
			// _mainPluginPanel
			// 
			resources.ApplyResources( this._mainPluginPanel, "_mainPluginPanel" );
			this._mainPluginPanel.BackColor = System.Drawing.SystemColors.ButtonHighlight;
			this._mainPluginPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this._mainPluginPanel.Name = "_mainPluginPanel";
			// 
			// CVKConfigForm
			// 
			resources.ApplyResources( this, "$this" );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add( this.splitContainer1 );
			this.DoubleBuffered = true;
			this.Name = "CVKConfigForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.splitContainer1.Panel1.ResumeLayout( false );
			this.splitContainer1.Panel2.ResumeLayout( false );
			this.splitContainer1.ResumeLayout( false );
			this.ResumeLayout( false );

        }
     
        #endregion

		private CVKMainPlugin _mainPluginPanel;
		private System.Windows.Forms.Panel _cvkTabContainer;
		private System.Windows.Forms.SplitContainer splitContainer1;



	}
}