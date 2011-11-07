using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IExplorer
{
    public partial class IEWindow : Form
    {
        public IEWindow()
        {
            InitializeComponent();

            // Permet de prendre en compte la propriété "CreateParams"
            this.SetStyle( ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                ControlStyles.UserMouse | ControlStyles.SupportsTransparentBackColor, true );
        }

        // Focus
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 134217728;
                return cp;
            }
        }

        #region Buttons Part
        
        // Browse Back
        private void BrowsBack_Click(object sender, EventArgs e)
        {
            SendKeys.Send("%{LEFT}");
        }

        // Browse Fwd
        private void BrowsFrWrd_Click(object sender, EventArgs e)
        {
            SendKeys.Send( "%{RIGHT}" );
        }

        // Add to Favorite (marque-page)
        private void AddFavorite_Click(object sender, EventArgs e)
        {
            SendKeys.Send("^(D)");
        }

        // Open NewTab (marque-page)
        private void NewTab_Click(object sender, EventArgs e)
        {
            SendKeys.Send("^(T)");
        }

        #endregion
    }
}
