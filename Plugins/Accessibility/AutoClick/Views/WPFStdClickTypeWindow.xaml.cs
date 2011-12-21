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
using System.Windows.Interop;
using CK.Interop;
using CK.Core;

namespace CK.Plugins.AutoClick.Views
{
    /// <summary>
    /// Interaction logic for WPFStandardClickTypeWindow.xaml
    /// </summary>
    public partial class WPFStdClickTypeWindow : Window
    {
        WindowInteropHelper _interopHelper;
        //double _proportions = 0;
        //bool _resizingWidth = false;
        //bool _resizingHeight = false;
        //bool allowResize = true;

        public WPFStdClickTypeWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += new MouseButtonEventHandler( WPFStdClickTypeWindow_PreviewMouseLeftButtonDown );
        }

        void WPFStdClickTypeWindow_PreviewMouseLeftButtonDown( object sender, MouseButtonEventArgs e )
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
            //allowResize = false;
        }

        //public const Int32 WM_EXITSIZEMOVE = 0x0232;
        //private IntPtr WinProc( IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, ref Boolean handled )
        //{
        //    IntPtr result = IntPtr.Zero;
        //    switch( msg )
        //    {
        //        case WM_EXITSIZEMOVE:
        //            {
        //                if( (_resizingHeight && _resizingWidth) || _resizingWidth )
        //                    this.Height = this.Width / _proportions;
        //                else if( _resizingHeight )
        //                    this.Width = this.Height * _proportions;

        //                //allowResize = true;
        //                //this.Measure( new Size( this.Width, this.Height ) );
        //                //this.ArrangeOverride( this.DesiredSize );
        //                //this.UpdateLayout();
        //                //allowResize = false;
        //                break;
        //            }
        //    }

        //    return result;
        //}

        //protected override Size ArrangeOverride( Size arrangeBounds )
        //{
        //    //if( allowResize )
        //        return base.ArrangeOverride( arrangeBounds );
        //    //else
        //    //    return base.ArrangeOverride( new Size( validatedSize ) );
        //}

        //Size validatedSize;

        //protected override void OnRenderSizeChanged( SizeChangedInfo sizeInfo )
        //{
        //    if( _proportions == 0 )
        //        _proportions = sizeInfo.NewSize.Width / sizeInfo.NewSize.Height;

        //    _resizingHeight = sizeInfo.HeightChanged;
        //    _resizingWidth = sizeInfo.WidthChanged;

        //    validatedSize = sizeInfo.NewSize;

        //    base.OnRenderSizeChanged( sizeInfo );
        //}
    }
}
