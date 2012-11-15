using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace CK.WordPredictor.UI.Helpers
{
    public class SelectedTextBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty SelectedTextProperty;

        private bool _internalChange;

        static SelectedTextBehavior()
        {
            SelectedTextProperty = DependencyProperty.Register( "SelectedText", typeof( string ), typeof( SelectedTextBehavior ), new PropertyMetadata( null, SelectedTextChanged ) );
        }

        public string SelectedText
        {
            get { return (string)GetValue( SelectedTextProperty ); }
            set { SetValue( SelectedTextProperty, value ); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private static void SelectedTextChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            var behavior = (SelectedTextBehavior)d;
            if( !behavior._internalChange )
            {
                behavior.AssociatedObject.SelectedText = e.NewValue as string;
            }
        }

        void AssociatedObject_SelectionChanged( object sender, RoutedEventArgs e )
        {
            _internalChange = true;
            SelectedText = AssociatedObject.SelectedText;
            _internalChange = false;
        }
    }
}
