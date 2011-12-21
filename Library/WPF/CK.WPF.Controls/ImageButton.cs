using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace CK.WPF.Controls
{
    public class ImageButton : Image
    {
        static ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( ImageButton ), new FrameworkPropertyMetadata( typeof( ImageButton ) ) );
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(ImageButton));

        public object CommandParameter
        {
            get { return (object)GetValue( CommandParameterProperty ); }
            set { SetValue( CommandParameterProperty, value ); }
        }
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register( "CommandParameter", typeof( object ), typeof( ImageButton ), new UIPropertyMetadata( null ) );

        protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
        {
            if( Command != null && Command.CanExecute( CommandParameter ) )
                Command.Execute( CommandParameter );
            base.OnMouseLeftButtonDown( e );
        }
    }
}
