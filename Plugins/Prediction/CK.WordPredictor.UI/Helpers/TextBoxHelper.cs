using System;
using System.Windows;
using System.Windows.Controls;

namespace CK.WordPredictor.UI.Helpers
{
    public class TextBoxHelpers
    {
        public static string GetSelectedText( DependencyObject obj )
        {
            return (string)obj.GetValue( SelectedTextProperty );
        }

        public static void SetSelectedText( DependencyObject obj, string value )
        {
            obj.SetValue( SelectedTextProperty, value );
        }

        // Using a DependencyProperty as the backing store for SelectedText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTextProperty =
        DependencyProperty.RegisterAttached(
                "SelectedText",
                typeof( string ),
                typeof( TextBoxHelpers ),
                new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedTextChanged ) );

        private static void SelectedTextChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
        {
            TextBox tb = obj as TextBox;
            if( tb != null )
            {
                if( e.OldValue == null && e.NewValue != null )
                {
                    tb.SelectionChanged += tb_SelectionChanged;
                }
                else if( e.OldValue != null && e.NewValue == null )
                {
                    tb.SelectionChanged -= tb_SelectionChanged;
                }

                string newValue = e.NewValue as string;

                if( newValue != null && newValue != tb.SelectedText )
                {
                    tb.SelectedText = newValue as string;
                }
            }
        }

        static void tb_SelectionChanged( object sender, RoutedEventArgs e )
        {
            TextBox tb = sender as TextBox;
            if( tb != null )
            {
                SetSelectedText( tb, tb.SelectedText );
            }
        }

        public static readonly DependencyProperty CaretIndex = 
            DependencyProperty.RegisterAttached( "CaretIndex", typeof( int ), typeof( TextBoxHelpers ),
            new FrameworkPropertyMetadata( 0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCaretIndexChanged ) );

        private static void OnCaretIndexChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            TextBox tb = d as TextBox;
            if( tb != null )
            {
                if( e.OldValue != e.NewValue )
                {
                    tb.CaretIndex = (int)e.NewValue;
                }
            }
        }

        [AttachedPropertyBrowsableForType( typeof( DependencyObject ) )]
        public static int GetCaretIndex( DependencyObject element )
        {
            if( element == null ) throw new ArgumentNullException( "element" );
            return (int)element.GetValue( CaretIndex );
        }

        public static void SetCaretIndex( DependencyObject element, int value )
        {
            if( element == null ) throw new ArgumentNullException( "element" );

            element.SetValue( CaretIndex, value );
        }
    }

}
