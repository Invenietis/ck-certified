namespace CK.Config.UI
{
    partial class CVKTabBase
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TabName = new System.Windows.Forms.Label();
            this.TabIcon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.TabIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // TabName
            // 
            this.TabName.BackColor = System.Drawing.Color.Transparent;
            this.TabName.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.TabName.Font = new System.Drawing.Font("Times New Roman", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TabName.Location = new System.Drawing.Point(0, 46);
            this.TabName.Name = "TabName";
            this.TabName.Size = new System.Drawing.Size(88, 21);
            this.TabName.TabIndex = 3;
            this.TabName.Text = "label1";
            this.TabName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TabIcon
            // 
            this.TabIcon.BackColor = System.Drawing.Color.Transparent;
            this.TabIcon.Location = new System.Drawing.Point(27, 10);
            this.TabIcon.Name = "TabIcon";
            this.TabIcon.Size = new System.Drawing.Size(32, 32);
            this.TabIcon.TabIndex = 4;
            this.TabIcon.TabStop = false;
            // 
            // CVKTabBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TabName);
            this.Controls.Add(this.TabIcon);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "CVKTabBase";
            this.Size = new System.Drawing.Size(88, 67);
            ((System.ComponentModel.ISupportInitialize)(this.TabIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label TabName;
        public System.Windows.Forms.PictureBox TabIcon;


    }
}
