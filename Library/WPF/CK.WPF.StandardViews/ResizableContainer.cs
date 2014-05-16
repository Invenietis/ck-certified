using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    //    public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register( "BorderThickness", typeof( bool ), typeof( ResizableContainer ) );
    //    public bool BorderThickness
    //    {
    //        get { return (bool)GetValue( BorderThicknessProperty ); }
    //        set { SetValue( BorderThicknessProperty, value ); }
    //    }

    //    public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register( "BorderBrush", typeof( bool ), typeof( ResizableContainer ) );
    //    public bool BorderBrush
    //    {
    //        get { return (bool)GetValue( BorderBrushProperty ); }
    //        set { SetValue( BorderBrushProperty, value ); }
    //    }
    }
}
