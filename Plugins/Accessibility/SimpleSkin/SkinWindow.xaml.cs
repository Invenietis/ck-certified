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

using System.Windows;
using System.Windows.Media;
using CK.Windows;
using SimpleSkin.Helpers;

namespace SimpleSkin
{
    /// <summary>
    /// Logique d'interaction pour SkinWindow.xaml
    /// </summary>
    public partial class SkinWindow : CKNoFocusWindow
    {
        //WindowResizer ob;
        public SkinWindow( NoFocusManager noFocusManager )
            : base( noFocusManager )
        {
            InitializeComponent();
            //ob = new WindowResizer(this);
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
    }
}

//public class WindowResizer
//{
//    WindowInteropHelper _window;

//    public WindowResizer( Window window )
//    {
//        _window = new WindowInteropHelper(window);
//    }


//    private void ResizeWindow( ResizeDirection direction )
//    {
//        Win.Functions.SendMessage( _window.Handle, Win.WM_SYSCOMMAND, (IntPtr)( Win.WMSysCommand.SIZE + (int)direction ), IntPtr.Zero );
//    }

//    public void Resize( object sender )
//    {
//        Rectangle clickedRectangle = sender as Rectangle;

//        switch( clickedRectangle.Name )
//        {
//            case "top":
//                ResizeWindow( ResizeDirection.Top );
//                break;
//            case "bottom":
//                ResizeWindow( ResizeDirection.Bottom );
//                break;
//            case "left":
//                ResizeWindow( ResizeDirection.Left );
//                break;
//            case "right":
//                ResizeWindow( ResizeDirection.Right );
//                break;
//            case "topLeft":
//                ResizeWindow( ResizeDirection.TopLeft );
//                break;
//            case "topRight":
//                ResizeWindow( ResizeDirection.TopRight );
//                break;
//            case "bottomLeft":
//                ResizeWindow( ResizeDirection.BottomLeft );
//                break;
//            case "bottomRight":
//                ResizeWindow( ResizeDirection.BottomRight );
//                break;
//            default:
//                break;
//        }
//    }

//    public enum ResizeDirection
//    {
//        Left = 1,
//        Right = 2,
//        Top = 3,
//        TopLeft = 4,
//        TopRight = 5,
//        Bottom = 6,
//        BottomLeft = 7,
//        BottomRight = 8,
//    }
//}
