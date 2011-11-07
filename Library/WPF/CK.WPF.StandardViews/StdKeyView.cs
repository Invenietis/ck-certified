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
using System.Diagnostics;
using System.Windows.Markup;

namespace CK.WPF.StandardViews
{
    [ContentProperty("UpLabel")]
    public class StdKeyView : Button
    {
        static StdKeyView()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( StdKeyView ), new FrameworkPropertyMetadata( typeof( StdKeyView ) ) );
        }

        public ICommand KeyDownCommand
        {
            get { return (ICommand)GetValue( KeyDownCommandProperty ); }
            set { SetValue( KeyDownCommandProperty, value ); }
        }
        public static readonly DependencyProperty KeyDownCommandProperty = 
        DependencyProperty.Register( "KeyDownCommand", typeof( ICommand ), typeof( StdKeyView ) );

        public ICommand KeyUpCommand
        {
            get { return (ICommand)GetValue( KeyUpCommandProperty ); }
            set { SetValue( KeyUpCommandProperty, value ); }
        }
        public static readonly DependencyProperty KeyUpCommandProperty = 
        DependencyProperty.Register( "KeyUpCommand", typeof( ICommand ), typeof( StdKeyView ) );

        public ICommand KeyPressedCommand
        {
            get { return (ICommand)GetValue( KeyPressedCommandProperty ); }
            set { SetValue( KeyPressedCommandProperty, value ); }
        }
        public static readonly DependencyProperty KeyPressedCommandProperty = 
        DependencyProperty.Register( "KeyPressedCommand", typeof( ICommand ), typeof( StdKeyView ) );

        public FrameworkElement CustomContent
        {
            get { return (Image)GetValue( CustomContentProperty ); }
            set { SetValue( CustomContentProperty, value ); }
        }
        public static readonly DependencyProperty CustomContentProperty = 
        DependencyProperty.Register( "CustomContent", typeof( FrameworkElement ), typeof( StdKeyView ) );

        public bool ShowLabel
        {
            get { return (bool)GetValue( ShowLabelProperty ); }
            set { SetValue( ShowLabelProperty, value ); }
        }
        public static readonly DependencyProperty ShowLabelProperty = 
        DependencyProperty.Register( "ShowLabel", typeof( bool ), typeof( StdKeyView ), new PropertyMetadata( true ) );

        public string UpLabel
        {
            get { return (string)GetValue( UpLabelProperty ); }
            set { SetValue( UpLabelProperty, value ); }
        }
        public static readonly DependencyProperty UpLabelProperty = 
        DependencyProperty.Register( "UpLabel", typeof( string ), typeof( StdKeyView ) );

        public string DownLabel
        {
            get { return (string)GetValue( DownLabelProperty ); }
            set { SetValue( DownLabelProperty, value ); }
        }
        public static readonly DependencyProperty DownLabelProperty = 
        DependencyProperty.Register( "DownLabel", typeof( string ), typeof( StdKeyView ) );

        public Brush HoverBackground
        {
            get { return (Brush)GetValue( HoverBackgroundProperty ); }
            set { SetValue( HoverBackgroundProperty, value ); }
        }
        public static readonly DependencyProperty HoverBackgroundProperty = 
        DependencyProperty.Register( "HoverBackground", typeof( Brush ), typeof( StdKeyView ) );

        public Brush PressedBackground
        {
            get { return (Brush)GetValue( PressedBackgroundProperty ); }
            set { SetValue( PressedBackgroundProperty, value ); }
        }
        public static readonly DependencyProperty PressedBackgroundProperty = 
        DependencyProperty.Register( "PressedBackground", typeof( Brush ), typeof( StdKeyView ) );

        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)GetValue( TextDecorationsProperty ); }
            set { SetValue( TextDecorationsProperty, value ); }
        }
        public static readonly DependencyProperty TextDecorationsProperty = 
        DependencyProperty.Register( "TextDecorations", typeof( TextDecorationCollection ), typeof( StdKeyView ) );

        protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
        {
            FireCommand( KeyDownCommand );
            base.OnMouseLeftButtonDown( e );
        }

        protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
        {
            FireCommand( KeyUpCommand );
            base.OnMouseLeftButtonUp( e );
        }

        void FireCommand( ICommand command )
        {
            if( command != null )
            {
                if( command.CanExecute( CommandParameter ) )
                    command.Execute( CommandParameter );
            }
        }
    }
}