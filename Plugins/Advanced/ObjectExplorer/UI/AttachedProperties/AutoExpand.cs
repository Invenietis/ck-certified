using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CK.Plugins.ObjectExplorer
{
    public class AutoExpand
    {
        public static bool GetIsEnabled( DependencyObject obj )
        {
            return (bool)obj.GetValue( IsEnabledProperty );
        }

        public static void SetIsEnabled( DependencyObject obj, bool value )
        {
            obj.SetValue( IsEnabledProperty, value );
        }

        public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached( "IsEnabled", typeof( bool ), typeof( AutoExpand ), new UIPropertyMetadata( false, OnIsEnabledChanged ) );

        static void OnIsEnabledChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            TreeViewItem item = sender as TreeViewItem;
            if( item != null )
            {
                if( (bool)e.NewValue )
                {
                    item.Selected += new RoutedEventHandler( OnSelected );
                    RealizeChildren( item as ItemsControl );
                }
                else
                {
                    item.Selected -= new RoutedEventHandler( OnSelected );
                }
            }
        }

        static void OnSelected( object sender, RoutedEventArgs e )
        {
            TreeViewItem source = e.OriginalSource as TreeViewItem;
            source.BringIntoView();
        }

        private static void RealizeChildren( ItemsControl container )
        {
            ItemContainerGenerator g = container.ItemContainerGenerator;
            if( g.Status == GeneratorStatus.NotStarted )
            {
                IItemContainerGenerator ig = (IItemContainerGenerator)g;
                using( ig.StartAt( new GeneratorPosition( -1, 0 ), GeneratorDirection.Forward, true ) )
                {
                    DependencyObject v = null;
                    while( (v = ig.GenerateNext()) != null ) ig.PrepareItemContainer( v );
                }
            }
        }
    }
}
