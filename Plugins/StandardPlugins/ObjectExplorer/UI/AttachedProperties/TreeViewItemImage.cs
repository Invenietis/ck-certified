using System.Windows.Media;
using System.Windows;

namespace CK.StandardPlugins.ObjectExplorer
{
    public class TreeViewItemImage
    {
        public static ImageSource GetImage( DependencyObject obj )
        {
            return (ImageSource)obj.GetValue( ImageProperty );
        }

        public static void SetImage( DependencyObject obj, ImageSource value )
        {
            obj.SetValue( ImageProperty, value );
        }

        public static readonly DependencyProperty ImageProperty =
        DependencyProperty.RegisterAttached( "Image", typeof( ImageSource ), typeof( TreeViewItemImage ) );
    }
}
