using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CK.Windows;

namespace CK.WindowManager
{
    /// <summary>
    /// Interaction logic for UnbindButtonView.xaml
    /// </summary>
    public partial class UnbindButtonView : CKNoFocusWindow
    {
        public UnbindButtonView( NoFocusManager noFocusManager )
            : base( noFocusManager ) 
        {
            InitializeComponent();
        }
    }
}
