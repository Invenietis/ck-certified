#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\UI\AttachedProperties\AutoExpand.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
