using System.Windows;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System;
using System.Windows.Interop;

namespace CK.Plugins.ObjectExplorer.UI.UserControls
{
    /// <summary>
    /// Interaction logic for LogConsoleWindow.xaml
    /// </summary>
    public partial class LogConsoleWindow : Window
    {
        public LogConsoleWindow()
        {
            InitializeComponent();
        }

        //protected override void OnClosing( CancelEventArgs e )
        //{
        //    base.OnClosing( e );
        //    e.Cancel = true;
        //}
    }
}
