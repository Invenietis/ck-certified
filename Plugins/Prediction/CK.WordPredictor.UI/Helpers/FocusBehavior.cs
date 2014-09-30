#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor.UI\Helpers\FocusBehavior.cs) is part of CiviKey. 
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
            AssociatedObject.GotKeyboardFocus += OnGotKeyboardFocus;
            AssociatedObject.LostKeyboardFocus += OnLostKeyboardFocus;
        }


        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotKeyboardFocus -= OnGotKeyboardFocus;
            AssociatedObject.PreviewGotKeyboardFocus -= OnGotKeyboardFocus;
        }

        private void OnLostKeyboardFocus( object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e )
        {
            IsKeyboardFocused = false;
        }

        void OnGotKeyboardFocus( object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e )
        {
            if( AssociatedObject == e.OldFocus ) return;
            if( AssociatedObject == e.NewFocus )
            {
                IsKeyboardFocused = true;
            }
        }

    }
}
