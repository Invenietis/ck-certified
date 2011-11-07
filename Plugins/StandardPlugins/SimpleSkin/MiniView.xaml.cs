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
using CK.Interop;
using CK.Core;

namespace SimpleSkin
{
    /// <summary>
    /// Interaction logic for MiniView.xaml
    /// </summary>
    public partial class MiniView : Window
    {
        Action _showSkin;
        WindowInteropHelper _interopHelper;
        HwndSourceHook _wndHook;
        IntPtr _lastFocused;
        bool _ncbuttondown;

        public MiniView( Action showSkin )
        {
            InitializeComponent();
            _showSkin = showSkin;

            move.MouseLeftButtonDown += new MouseButtonEventHandler( OnMoveDown );
        }

        void OnMoveDown( object sender, MouseButtonEventArgs e )
        {
            GetFocus();
            DragMove();
            ReleaseFocus();
        }

        void GetFocus()
        {
            _lastFocused = Win.Functions.GetForegroundWindow();
            Win.Functions.SetForegroundWindow( _interopHelper.Handle );
        }

        void ReleaseFocus()
        {
            Win.Functions.SetForegroundWindow( _lastFocused );
        }

        protected override void OnSourceInitialized( EventArgs e )
        {
            _interopHelper = new WindowInteropHelper( this );

            Win.Functions.SetWindowLong( _interopHelper.Handle, Win.WindowLongIndex.GWL_EXSTYLE, (long)Win.WS_EX.NOACTIVATE );

            HwndSource mainWindowSrc = HwndSource.FromHwnd( _interopHelper.Handle );

            _wndHook = new HwndSourceHook( WndProc );
            mainWindowSrc.AddHook( _wndHook );

            base.OnSourceInitialized( e );
        }

        IntPtr WndProc( IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        {
            switch( (Win.WM)msg )
            {
                case Win.WM.NCLBUTTONDOWN:
                    _ncbuttondown = true;
                    GetFocus();
                    break;
                case Win.WM.NCMOUSEMOVE:
                    if( _ncbuttondown )
                    {
                        ReleaseFocus();
                        _ncbuttondown = false;
                    }
                    break;
            }
            return hWnd;
        }

        protected override void OnPreviewMouseDoubleClick( MouseButtonEventArgs e )
        {
            e.Handled = true;
            _showSkin.Invoke();
            base.OnPreviewMouseDoubleClick( e );
        }
    }
}
