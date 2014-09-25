#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\UI\AttachedProperties\TypeAheadBehaviour.cs) is part of CiviKey. 
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

namespace CK.Plugins.ObjectExplorer
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
