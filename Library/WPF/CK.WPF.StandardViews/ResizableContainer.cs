using System.Windows;
using System.Windows.Controls;

namespace CK.WPF.StandardViews
{
    public class ResizableContainer : Control
    {
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register( "Content", typeof( object ), typeof( ResizableContainer ) );
        public object Content
        {
            get { return GetValue( ContentProperty ); }
            set { SetValue( ContentProperty, value ); }
        }
    }
}
