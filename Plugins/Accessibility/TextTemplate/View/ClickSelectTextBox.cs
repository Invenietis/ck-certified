using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TextTemplate
{
    public class ClickSelectTextBox : TextBox
    {
        public string Placeholder
        {
            get { return (string)GetValue( PlaceholderProperty ); }
            set { SetValue( PlaceholderProperty, value ); }
        }
        public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register( "Placeholder", typeof( string ), typeof( ClickSelectTextBox ) );

        public ClickSelectTextBox()
        {
            AddHandler( PreviewMouseLeftButtonDownEvent,
              new MouseButtonEventHandler( SelectivelyIgnoreMouseButton ), true );
            AddHandler( GotKeyboardFocusEvent,
              new RoutedEventHandler( SelectAllText ), true );
            AddHandler( MouseDoubleClickEvent,
              new RoutedEventHandler( SelectAllText ), true );
        }
        public ClickSelectTextBox(string placeholder) : this()
        {
            Text = placeholder;
            Placeholder = placeholder;
        }
        private static void SelectivelyIgnoreMouseButton( object sender,
                                                         MouseButtonEventArgs e )
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while( parent != null && !(parent is TextBox) )
                parent = VisualTreeHelper.GetParent( parent );

            if( parent != null )
            {
                var textBox = (ClickSelectTextBox)parent;
                if( !textBox.IsKeyboardFocusWithin )
                {
                    // If the text box is not yet focussed, give it the focus and
                    // stop further processing of this click event.
                    if( textBox.Text == textBox.Placeholder )
                    {
                        textBox.Focus();
                        e.Handled = true;
                    }
                }
            }
        }

        private static void SelectAllText( object sender, RoutedEventArgs e )
        {
            var textBox = e.OriginalSource as TextBox;
            if( textBox != null )
                textBox.SelectAll();
        }
    }
}
