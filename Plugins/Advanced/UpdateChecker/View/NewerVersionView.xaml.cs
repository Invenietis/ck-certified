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

namespace UpdateChecker.View
{
    /// <summary>
    /// Interaction logic for NewerVersionView.xaml
    /// </summary>
    public partial class NewerVersionView : Window
    {
        public NewerVersionView()
        {
            InitializeComponent();
        }

        public void SetBrowserContent( string content )
        {
            _browser.NavigateToString( content );
        }
    }
}
