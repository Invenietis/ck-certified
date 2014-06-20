#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor.UI\Helpers\SelectedTextBehavior.cs) is part of CiviKey. 
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

using System.Windows;
using System.Windows.Controls;
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
