using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Media;

namespace CK.WPF.StandardViews
{
    public class StdKeyboardView : Control
    {
        static StdKeyboardView()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( StdKeyboardView ), new FrameworkPropertyMetadata( typeof( StdKeyboardView ) ) );
        }

        #region Dependency properties

        public IEnumerable Keys
        {
            get { return (IEnumerable)GetValue( KeysProperty ); }
            set { SetValue( KeysProperty, value ); }
        }
        public static readonly DependencyProperty KeysProperty = 
            DependencyProperty.Register( "Keys", typeof( IEnumerable ), typeof( StdKeyboardView ) );

        public Brush OutsideBorderColor
        {
            get { return (Brush)GetValue( OutsideBorderColorProperty ); }
            set { SetValue( OutsideBorderColorProperty, value ); }
        }
        public static readonly DependencyProperty OutsideBorderColorProperty = 
            DependencyProperty.Register( "OutsideBorderColor", typeof( Brush ), typeof( StdKeyboardView ) );

        public Thickness OutsideBorderThickness
        {
            get { return (Thickness)GetValue( OutsideBorderThicknessProperty ); }
            set { SetValue( OutsideBorderThicknessProperty, value ); }
        }
        public static readonly DependencyProperty OutsideBorderThicknessProperty = 
            DependencyProperty.Register( "OutsideBorderThickness", typeof( Thickness ), typeof( StdKeyboardView ) );

        public Brush InsideBorderColor
        {
            get { return (Brush)GetValue( InsideBorderColorProperty ); }
            set { SetValue( InsideBorderColorProperty, value ); }
        }
        public static readonly DependencyProperty InsideBorderColorProperty = 
            DependencyProperty.Register( "InsideBorderColor", typeof( Brush ), typeof( StdKeyboardView ) );

        public Thickness InsideBorderThickness
        {
            get { return (Thickness)GetValue( InsideBorderThicknessProperty ); }
            set { SetValue( InsideBorderThicknessProperty, value ); }
        }
        public static readonly DependencyProperty InsideBorderThicknessProperty = 
            DependencyProperty.Register( "InsideBorderThickness", typeof( Thickness ), typeof( StdKeyboardView ) );

        #endregion
    }
}
