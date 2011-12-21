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
using System.Windows.Shapes;

namespace CK.AppRecovery
{
    /// <summary>
    /// Interaction logic for CrashLogWindow.xaml
    /// </summary>
    internal partial class CrashLogWindow : Window
    {
        public CrashLogWindow()
        {
        }

        public CrashLogWindow( CrashLogWindowViewModel dataContext )
        {
            DataContext = dataContext;
            InitializeComponent();
        }

        public CrashLogWindowViewModel Uploader
        {
            get { return (CrashLogWindowViewModel)DataContext; }
        }

        private void send_Click( object sender, RoutedEventArgs e )
        {
            Uploader.StartUpload();
        }

    }
}
