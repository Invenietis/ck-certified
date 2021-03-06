#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor.UI\Helpers\CaretIndexBehavior.cs) is part of CiviKey. 
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace CK.WordPredictor.UI.Helpers
{
    public class CaretIndexBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty CaretPositionProperty;

        private bool _internalChange;

        static CaretIndexBehavior()
        {
            CaretPositionProperty = DependencyProperty.Register( "CaretPosition", typeof( int ), typeof( CaretIndexBehavior ), new PropertyMetadata( 0, OnCaretPositionChanged ) );
        }

        public int CaretPosition
        {
            get { return Convert.ToInt32( GetValue( CaretPositionProperty ) ); }
            set { SetValue( CaretPositionProperty, value ); }
        }


        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyUp += OnKeyUp;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_MouseUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.KeyUp -= OnKeyUp;
            AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_MouseUp;
        } 


        private static void OnCaretPositionChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            var behavior = (CaretIndexBehavior)d;
            if( !behavior._internalChange )
            {
                behavior.AssociatedObject.CaretIndex = Convert.ToInt32( e.NewValue );
            }
        }

        void AssociatedObject_MouseUp( object sender, MouseButtonEventArgs e )
        {
            MoveCaretInternal();
        }

        private void OnKeyUp( object sender, KeyEventArgs e )
        {
            MoveCaretInternal();
        }

        private void MoveCaretInternal()
        {
            _internalChange = true;
            CaretPosition = AssociatedObject.CaretIndex;
            _internalChange = false;
        }
    }
}
