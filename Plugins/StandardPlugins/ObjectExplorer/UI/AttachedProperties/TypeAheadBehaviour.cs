using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace CK.StandardPlugins.ObjectExplorer
{
    public class TypeAheadBehaviour
    {
        public static bool GetIsEnabled( DependencyObject obj )
        {
            return (bool)obj.GetValue( IsEnabledProperty );
        }

        public static void SetIsEnabled( DependencyObject obj, bool value )
        {
            obj.SetValue( IsEnabledProperty, value );
        }

        // Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached( "IsEnabled", typeof( bool ), typeof( TypeAheadBehaviour ), new UIPropertyMetadata( false, OnIsEnabledChanged ) );

        static void OnIsEnabledChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            TextBox tb = (TextBox)sender;
            bool isEnabled = (bool)e.NewValue;

            if( isEnabled )
            {
                tb.GotFocus += new RoutedEventHandler( RemoveContent );
                tb.LostFocus += new RoutedEventHandler( ResetDefaultContent );
            }
            else
            {
                tb.GotFocus -= new RoutedEventHandler( RemoveContent );
                tb.LostFocus -= new RoutedEventHandler( ResetDefaultContent );
            }
        }

        static void ResetDefaultContent( object sender, RoutedEventArgs e )
        {
            TextBox tb = (TextBox)sender;
            if( tb.Text == "" )
                tb.Text = "Search";
        }

        static void RemoveContent( object sender, RoutedEventArgs e )
        {
            TextBox tb = (TextBox)sender;
            if( tb.Text == "Search" )
                tb.Text = "";
        }
    }
}
