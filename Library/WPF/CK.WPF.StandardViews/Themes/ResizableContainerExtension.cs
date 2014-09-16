using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using CK.Windows.Interop;

namespace CK.WPF.StandardViews
{
    public partial class ResizableContainerExtension : ResourceDictionary
    {
        WindowResizer _ob;
        public ResizableContainerExtension()
        {
        }

        private void Resize( object sender, MouseButtonEventArgs e )
        {
            Rectangle rec = (Rectangle)sender;
            DependencyObject parent = VisualTreeHelper.GetParent( rec );
           
            if( _ob == null )
            { 
                while( parent != null && !(parent is Window) )
                    parent = VisualTreeHelper.GetParent( parent );
                _ob = new WindowResizer( (Window)parent );
            }

            _ob.Resize( sender );
        }
    }

    public class WindowResizer
    {
        WindowInteropHelper _window;

        public WindowResizer( Window window )
        {
            _window = new WindowInteropHelper( window );
        }


        private void ResizeWindow( ResizeDirection direction )
        {
            Win.Functions.SendMessage( _window.Handle, Win.WM_SYSCOMMAND, (IntPtr)(Win.WMSysCommand.SIZE + (int)direction), IntPtr.Zero );
        }

        public void Resize( object sender )
        {
            Rectangle clickedRectangle = sender as Rectangle;

            switch( clickedRectangle.Name )
            {
                case "top":
                    ResizeWindow( ResizeDirection.Top );
                    break;
                case "bottom":
                    ResizeWindow( ResizeDirection.Bottom );
                    break;
                case "left":
                    ResizeWindow( ResizeDirection.Left );
                    break;
                case "right":
                    ResizeWindow( ResizeDirection.Right );
                    break;
                case "topLeft":
                    ResizeWindow( ResizeDirection.TopLeft );
                    break;
                case "topRight":
                    ResizeWindow( ResizeDirection.TopRight );
                    break;
                case "bottomLeft":
                    ResizeWindow( ResizeDirection.BottomLeft );
                    break;
                case "bottomRight":
                    ResizeWindow( ResizeDirection.BottomRight );
                    break;
                default:
                    break;
            }
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
    }

}
