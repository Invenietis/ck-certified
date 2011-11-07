using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using CK.Interop;
using System.Runtime.InteropServices;
using CK.Core;
using System.Windows.Media;
using System.Diagnostics;

namespace Host
{
    public partial class AppView : Window
    {
        WindowInteropHelper _interopHelper;
        HwndSourceHook _wndHook;
        Win.Rect _rWindow;

        //protected override void OnSourceInitialized( EventArgs e )
        //{
        //    _rWindow = new Win.Rect() { Left = 10, Right = 10, Bottom = 600, Top = 400 };
        //    _interopHelper = new WindowInteropHelper( this );
        //    _wndHook = new HwndSourceHook( WndProc );
        //    HwndSource mainWindowSrc = HwndSource.FromHwnd( _interopHelper.Handle );
        //    mainWindowSrc.AddHook( _wndHook );
        //    base.OnSourceInitialized( e );
        //}

        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );
            taskbarIcon.Dispose();
        }

        public new AppViewModel DataContext { get { return (AppViewModel)base.DataContext; } }


        //IntPtr WndProc( IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        //{
        //    switch( (Win.WM)msg )
        //    {
        //        //case Win.WM.WINDOWPOSCHANGING:
        //        //    {
        //        //        var sizeMax = PresentationSource.FromVisual( this ).CompositionTarget.TransformToDevice.Transform( new Point( MaxWidth, MaxHeight ) );
        //        //        Win.Size sMax = new Win.Size() 
        //        //        {
        //        //            // Use the same rounding as ElementHost.
        //        //            Cx = (int)Math.Max( int.MinValue, Math.Min( int.MaxValue, sizeMax.X ) ),
        //        //            Cy = (int)Math.Max( int.MinValue, Math.Min( int.MaxValue, sizeMax.Y ) ),
        //        //        };  
        //        //        Win.WindowPos p = (Win.WindowPos)Marshal.PtrToStructure( lParam, typeof( Win.WindowPos ) );
        //        //        bool changed = false;
        //        //        if( p.Cx > sMax.Cx ) { p.Cx = sMax.Cx; changed = true; }
        //        //        if( p.Cy > sMax.Cy ) { p.Cy = sMax.Cy; changed = true; }
        //        //        if( changed )
        //        //        {
        //        //            Marshal.StructureToPtr( p, lParam, false );
        //        //        }
        //        //        break;
        //        //    }
        //        case Win.WM.SYSCOMMAND:
        //            {
        //                if( wParam == (IntPtr)Win.WMSysCommand.MAXIMIZE )
        //                {
        //                    Win.Rect r = new Win.Rect();
        //                    if( Win.Functions.GetWindowRect( hWnd, out r ) )
        //                    {
        //                        System.Windows.Forms.Screen parent = System.Windows.Forms.Screen.FromRectangle( new System.Drawing.Rectangle( r.Left, r.Top, r.Width, r.Height ) );
        //                        int midX = parent.WorkingArea.Width / 2;
        //                        int midY = parent.WorkingArea.Height / 2;

        //                        var sizeMax = PresentationSource.FromVisual( this ).CompositionTarget.TransformToDevice.Transform( new Point( MaxWidth, MaxHeight ) );
        //                        int cx = (int)Math.Max( int.MinValue, Math.Min( int.MaxValue, sizeMax.X ) );
        //                        int cy = (int)Math.Max( int.MinValue, Math.Min( int.MaxValue, sizeMax.Y ) );

        //                        Win.Functions.SetWindowPos( hWnd, (IntPtr)Win.SpecialWindowHandles.TOP, midX - cx / 2, midY - cy / 2, cx, cy, 0 );
        //                        handled = true;
        //                    }
        //                }
        //                break;
        //            }
        //        case Win.WM.SIZE:
        //            if( wParam == (IntPtr)Win.WMSize.MINIMIZED )
        //            {
        //                if( !DataContext.ShowTaskbarIcon ) Win.Functions.SetWindowPos( hWnd, IntPtr.Zero, 0, 0, 0, 0, Win.SetWindowPosFlags.HideWindow );
        //            }
        //            break;
        //    }
        //    return IntPtr.Zero;
        //}

 
        //void ConstrainMove( IntPtr pRect )
        //{
        //    Win.Rect r = (Win.Rect)Marshal.PtrToStructure( pRect, typeof(Win.Rect) );
        //    if( !_rWindow.Contains( r ) )
        //    {
        //        r = CheckPosition( r );

        //        Marshal.StructureToPtr( r, pRect, false );
        //    }
        //}

        //private Win.Rect CheckPosition( Win.Rect r )
        //{
        //    if( r.Left < _rWindow.Left )
        //    {
        //        r.Right += _rWindow.Left - r.Left;
        //        r.Left = _rWindow.Left;
        //    }
        //    else if( r.Right > _rWindow.Right )
        //    {
        //        r.Left += _rWindow.Right - r.Right;
        //        r.Right = _rWindow.Right;
        //    }

        //    if( r.Top < _rWindow.Top )
        //    {
        //        r.Bottom += _rWindow.Top - r.Top;
        //        r.Top = _rWindow.Top;
        //    }
        //    else if( r.Bottom > _rWindow.Bottom )
        //    {
        //        r.Top += _rWindow.Bottom - r.Bottom;
        //        r.Bottom = _rWindow.Bottom;
        //    }
        //    return r;
        //}

        //void ConstrainSize( IntPtr pRect )
        //{
        //    Win.Rect r = (Win.Rect)Marshal.PtrToStructure( pRect, typeof( Win.Rect ) );

        //    if( !_rWindow.Contains( r ) )
        //    {
        //        if( r.Left < _rWindow.Left )
        //        {
        //            r.Left = _rWindow.Left;
        //        }
        //        else if( r.Right > _rWindow.Right )
        //        {
        //            r.Right = _rWindow.Right;
        //        }

        //        if( r.Top < _rWindow.Top )
        //        {
        //            r.Top = _rWindow.Top;
        //        }
        //        else if( r.Bottom > _rWindow.Bottom )
        //        {
        //            r.Bottom = _rWindow.Bottom;
        //        }
        //        Marshal.StructureToPtr( r, pRect, false );
        //    }
        //}

    }
}
