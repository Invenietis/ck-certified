using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace CK.WordPredictor.UI
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

    public class SetCaretIndexBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty CaretPositionProperty;
        public static readonly DependencyProperty SelectedTextProperty;

        private bool _internalChange;

        static SetCaretIndexBehavior()
        {
            SelectedTextProperty = DependencyProperty.Register( "SelectedText", typeof( string ), typeof( SetCaretIndexBehavior ), new PropertyMetadata( null, SelectedTextChanged ) );
            CaretPositionProperty = DependencyProperty.Register( "CaretPosition", typeof( int ), typeof( SetCaretIndexBehavior ), new PropertyMetadata( 0, OnCaretPositionChanged ) );
        }

        public int CaretPosition
        {
            get { return Convert.ToInt32( GetValue( CaretPositionProperty ) ); }
            set { SetValue( CaretPositionProperty, value ); }
        }

        public string SelectedText
        {
            get { return (string)GetValue( SelectedTextProperty ); }
            set { SetValue( SelectedTextProperty, value ); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyUp += OnKeyUp;
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }


        private static void OnCaretPositionChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            var behavior = (SetCaretIndexBehavior)d;
            if( !behavior._internalChange )
            {
                behavior.AssociatedObject.CaretIndex = Convert.ToInt32( e.NewValue );
            }
        }

        private static void SelectedTextChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            var behavior = (SetCaretIndexBehavior)d;
            if( !behavior._internalChange )
            {
                behavior.AssociatedObject.SelectedText = e.NewValue as string;
            }
        }

        private void OnKeyUp( object sender, KeyEventArgs e )
        {
            _internalChange = true;
            CaretPosition = AssociatedObject.CaretIndex;
            _internalChange = false;
        }

        void AssociatedObject_SelectionChanged( object sender, RoutedEventArgs e )
        {
            _internalChange = true;
            SelectedText = AssociatedObject.SelectedText;
            _internalChange = false;
        }

    }
}
