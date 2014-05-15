#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\SkinWindow.xaml.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Windows;
using CK.Windows;
using System.Windows.Media;
using SimpleSkin.Helpers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using CK.Interop;
using System.Diagnostics;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace SimpleSkin
{
    /// <summary>
    /// Logique d'interaction pour SkinWindow.xaml
    /// </summary>
    public partial class SkinWindow : CKNoFocusWindow
    {
        WindowResizer ob;
        public SkinWindow( NoFocusManager noFocusManager )
            : base( noFocusManager )
        {
            InitializeComponent();
            ob = new WindowResizer(this);
        }

        protected override bool IsDraggableVisual( DependencyObject visualElement )
        {
            FrameworkElement border = visualElement as FrameworkElement;
            //Allows drag and drop when the background is set
            if( border != null && border.Name == "InsideBorder" ) return true;
            if( DraggableVisualAttachedProperty.GetDraggableVisual( visualElement ) ) return true;
            var parent = VisualTreeHelper.GetParent( visualElement );
            return parent is SkinWindow || base.IsDraggableVisual( visualElement );
        }

        // 
        private void Resize(object sender, MouseButtonEventArgs e)
        {
            ob.resizeWindow(sender);
        }

        private void DisplayResizeCursor(object sender, MouseEventArgs e)
        {
            ob.displayResizeCursor(sender);
        }

        private void ResetCursor(object sender, MouseEventArgs e)
        {
            ob.resetCursor();
        }

        private void Drag(object sender, MouseButtonEventArgs e)
        {
            ob.dragWindow();
        }
    }
}

class WindowResizer
{
    private const int WM_SYSCOMMAND = 0x112;
    private HwndSource hwndSource;
    Window activeWin;

    public WindowResizer( Window activeW )
    {
        activeWin = activeW as Window;

        activeWin.SourceInitialized += new EventHandler( InitializeWindowSource );
    }


    public void resetCursor()
    {
        if( Mouse.LeftButton != MouseButtonState.Pressed )
        {
            activeWin.Cursor = Cursors.Arrow;
        }
    }

    public void dragWindow()
    {
        activeWin.DragMove();
    }

    private void InitializeWindowSource( object sender, EventArgs e )
    {
        hwndSource = PresentationSource.FromVisual( (Visual)sender ) as HwndSource;
        hwndSource.AddHook( new HwndSourceHook( WndProc ) );
    }

    IntPtr retInt = IntPtr.Zero;

    private IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
    {
        Debug.WriteLine( "WndProc messages: " + msg.ToString() );
        //
        // Check incoming window system messages
        //
        if( msg == WM_SYSCOMMAND )
        {
            Debug.WriteLine( "WndProc messages: " + msg.ToString() );
        }

        return IntPtr.Zero;
    }

    public enum ResizeDirection
    {
        Left = 1,
        Right = 2,
        Top = 3,
        TopLeft = 4,
        TopRight = 5,
        Bottom = 6,
        BottomLeft = 7,
        BottomRight = 8,
    }

    [System.Runtime.InteropServices.DllImport( "user32.dll", CharSet = CharSet.Auto )]
    private static extern IntPtr SendMessage( IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam );


    private void ResizeWindow( ResizeDirection direction )
    {
        SendMessage( hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero );
    }


    public void resizeWindow( object sender )
    {
        Rectangle clickedRectangle = sender as Rectangle;

        switch( clickedRectangle.Name )
        {
            case "top":
                activeWin.Cursor = Cursors.SizeNS;
                ResizeWindow( ResizeDirection.Top );
                break;
            case "bottom":
                activeWin.Cursor = Cursors.SizeNS;
                ResizeWindow( ResizeDirection.Bottom );
                break;
            case "left":
                activeWin.Cursor = Cursors.SizeWE;
                ResizeWindow( ResizeDirection.Left );
                break;
            case "right":
                activeWin.Cursor = Cursors.SizeWE;
                ResizeWindow( ResizeDirection.Right );
                break;
            case "topLeft":
                activeWin.Cursor = Cursors.SizeNWSE;
                ResizeWindow( ResizeDirection.TopLeft );
                break;
            case "topRight":
                activeWin.Cursor = Cursors.SizeNESW;
                ResizeWindow( ResizeDirection.TopRight );
                break;
            case "bottomLeft":
                activeWin.Cursor = Cursors.SizeNESW;
                ResizeWindow( ResizeDirection.BottomLeft );
                break;
            case "bottomRight":
                activeWin.Cursor = Cursors.SizeNWSE;
                ResizeWindow( ResizeDirection.BottomRight );
                break;
            default:
                break;
        }
    }


    public void displayResizeCursor( object sender )
    {
        Rectangle clickedRectangle = sender as Rectangle;

        switch( clickedRectangle.Name )
        {
            case "top":
                activeWin.Cursor = Cursors.SizeNS;
                break;
            case "bottom":
                activeWin.Cursor = Cursors.SizeNS;
                break;
            case "left":
                activeWin.Cursor = Cursors.SizeWE;
                break;
            case "right":
                activeWin.Cursor = Cursors.SizeWE;
                break;
            case "topLeft":
                activeWin.Cursor = Cursors.SizeNWSE;
                break;
            case "topRight":
                activeWin.Cursor = Cursors.SizeNESW;
                break;
            case "bottomLeft":
                activeWin.Cursor = Cursors.SizeNESW;
                break;
            case "bottomRight":
                activeWin.Cursor = Cursors.SizeNWSE;
                break;
            default:
                break;
        }
    }
}
