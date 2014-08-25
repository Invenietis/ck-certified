#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\TextTemplate\View\ClickSelectTextBox.cs) is part of CiviKey. 
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
