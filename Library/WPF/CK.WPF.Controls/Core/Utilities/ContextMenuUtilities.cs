#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.Controls\Core\Utilities\ContextMenuUtilities.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Windows;

namespace Microsoft.Windows.Controls.Core.Utilities
{
    public class ContextMenuUtilities
    {
        public static readonly DependencyProperty OpenOnMouseLeftButtonClickProperty = DependencyProperty.RegisterAttached( "OpenOnMouseLeftButtonClick", typeof( bool ), typeof( ContextMenuUtilities ), new FrameworkPropertyMetadata( false, OpenOnMouseLeftButtonClickChanged ) );
        public static void SetOpenOnMouseLeftButtonClick( FrameworkElement element, bool value )
        {
            element.SetValue( OpenOnMouseLeftButtonClickProperty, value );
        }
        public static bool GetOpenOnMouseLeftButtonClick( FrameworkElement element )
        {
            return (bool)element.GetValue( OpenOnMouseLeftButtonClickProperty );
        }

        public static void OpenOnMouseLeftButtonClickChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
        {
            var control = (FrameworkElement)sender;
            if( (bool)e.NewValue )
            {
                control.PreviewMouseLeftButtonDown += ( s, args ) =>
                {
                    if( control.ContextMenu != null )
                    {
                        control.ContextMenu.PlacementTarget = control;
                        control.ContextMenu.IsOpen = true;
                    }
                };
            }
            //TODO: remove handler when set to false
        }
    }
}
