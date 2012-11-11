using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace CK.WordPredictor.UI.Helpers
{
    public class FocusBehavior : Behavior<TextBoxBase>
    {
        public static readonly DependencyProperty IsKeyboardFocusedProperty;

        static FocusBehavior()
        {
            IsKeyboardFocusedProperty = DependencyProperty.Register( "IsKeyboardFocused", typeof( bool ), typeof( FocusBehavior ) );
        }

        public bool IsKeyboardFocused
        {
            get { return (bool)GetValue( IsKeyboardFocusedProperty ); }
            set { SetValue( IsKeyboardFocusedProperty, value ); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewGotKeyboardFocus += OnPreviewGotKeyboardFocus;
            AssociatedObject.PreviewLostKeyboardFocus += OnPreviewLostKeyboardFocus;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewGotKeyboardFocus -= OnPreviewGotKeyboardFocus;
            AssociatedObject.PreviewLostKeyboardFocus -= OnPreviewLostKeyboardFocus;
        }

        private void OnPreviewLostKeyboardFocus( object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e )
        {
            IsKeyboardFocused = false;
        }

        void OnPreviewGotKeyboardFocus( object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e )
        {
            if( AssociatedObject == e.OldFocus ) return;
            if( AssociatedObject == e.NewFocus )
            {
                IsKeyboardFocused = true;
            }
        }

    }
}
