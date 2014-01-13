using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeyboardEditor.ViewModels
{
    public class ActionControl : Control
    {
        static ActionControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( ActionControl ), new FrameworkPropertyMetadata( typeof( ActionControl ) ) );
        }

        public ICommand Action
        {
            get { return (ICommand)GetValue( ActionProperty ); }
            set { SetValue( ActionProperty, value ); }
        }
        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.Register( "Action", typeof( ICommand ), typeof( ActionControl ) );

        public object ActionParameter
        {
            get { return (object)GetValue( ActionParameterProperty ); }
            set { SetValue( ActionParameterProperty, value ); }
        }
        public static readonly DependencyProperty ActionParameterProperty =
            DependencyProperty.Register( "ActionParameter", typeof( object ), typeof( ActionControl ) );


        public string Text
        {
            get { return (string)GetValue( TextProperty ); }
            set { SetValue( TextProperty, value ); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register( "Text", typeof( string ), typeof( ActionControl ) );

        public string TooltipText
        {
            get { return (string)GetValue( TooltipTextProperty ); }
            set { SetValue( TooltipTextProperty, value ); }
        }
        public static readonly DependencyProperty TooltipTextProperty =
            DependencyProperty.Register( "TooltipText", typeof( string ), typeof( ActionControl ) );

        public string ImageSource
        {
            get { return GetValue( ImageSourceProperty ).ToString(); }
            set { SetValue( ImageSourceProperty, value ); }
        }
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register( "ImageSource", typeof( string ), typeof( ActionControl ) );
    }
}
