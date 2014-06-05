using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
