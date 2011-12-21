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
using System.Windows.Interop;
using CK.Core;
using CK.Interop;

namespace CK.Plugins.AutoClick.Views
{
    /// <summary>
    /// Interaction logic for AutoClickEditorWindow.xaml
    /// </summary>
    public partial class AutoClickEditorWindow : Window
    {
        WindowInteropHelper _interopHelper;

        public AutoClickEditorWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += new MouseButtonEventHandler( WPFStdClickTypeWindow_MouseLeftButtonDown );
        }

        void WPFStdClickTypeWindow_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
            DragMove();
        }

        protected override void OnSourceInitialized( EventArgs e )
        {
            _interopHelper = new WindowInteropHelper( this );            

            HwndSource mainWindowSrc = HwndSource.FromHwnd( _interopHelper.Handle );

            mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb( 0, 0, 0, 0 );
            mainWindowSrc.CompositionTarget.RenderMode = RenderMode.Default;

            if( OSVersionInfo.IsWindowsVistaOrGreater && Dwm.Functions.IsCompositionEnabled() )
            {
                Win.Margins m = new Win.Margins() { LeftWidth = -1, RightWidth = -1, TopHeight = -1, BottomHeight = -1 };
                Dwm.Functions.ExtendFrameIntoClientArea( _interopHelper.Handle, ref m );
            }
            else
            {
                this.Background = new SolidColorBrush( Colors.WhiteSmoke );
            }


            base.OnSourceInitialized( e );
        }
    }
}
