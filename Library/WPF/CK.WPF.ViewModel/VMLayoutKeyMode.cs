#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.ViewModel\VMKey.cs) is part of CiviKey. 
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
using System.Windows.Input;
using System.Collections.Generic;
using CK.Core;
using CK.Keyboard.Model;

namespace CK.WPF.ViewModel
{
    public abstract class VMLayoutKeyMode<TC, TB, TZ, TK, TKM, TLKM> : VMContextElement<TC, TB, TZ, TK, TKM, TLKM>
        where TC : VMContext<TC, TB, TZ, TK, TKM, TLKM>
        where TB : VMKeyboard<TC, TB, TZ, TK, TKM, TLKM>
        where TZ : VMZone<TC, TB, TZ, TK, TKM, TLKM>
        where TK : VMKey<TC, TB, TZ, TK, TKM, TLKM>
        where TKM : VMKeyMode<TC, TB, TZ, TK, TKM, TLKM>
        where TLKM : VMLayoutKeyMode<TC, TB, TZ, TK, TKM, TLKM>
    {
        ILayoutKeyMode _model;
        private VMKey<TC, TB, TZ, TK, TKM, TLKM> ActualParent { get { return Parent as VMKey<TC, TB, TZ, TK, TKM, TLKM>; } }

        public VMLayoutKeyMode( TC context, ILayoutKeyMode keyMode )
            : base( context )
        {
            _model = keyMode;
        }

        public void TriggerPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );
        }

        /// <summary>
        /// Gets the X coordinate of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int X
        {
            get { return _model.X; }
            set
            {
                _model.X = value;
                OnPropertyChanged( "X" );
            }
        }

        /// <summary>
        /// Gets the Y coordinate of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int Y
        {
            get { return _model.Y; }
            set
            {
                _model.Y = value;
                OnPropertyChanged( "Y" );
            }
        }

        /// <summary>
        /// Gets or sets the width of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int Width
        {
            get { return _model.Width; }
            set
            {
                _model.Width = value;
                OnPropertyChanged( "Width" );
            }
        }

        /// <summary>
        /// Gets or sets the height of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int Height
        {
            get { return _model.Height; }
            set
            {
                _model.Height = value;
                OnPropertyChanged( "Height" );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this actual key is visible or not, for the current <see cref="IKeyMode"/>.
        /// </summary>
        public Visibility Visible
        {
            get { return IsVisible ? Visibility.Visible : Visibility.Collapsed; }
            set
            {
                IsVisible = ( value == Visibility.Visible );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this actual key is visible or not, for the current <see cref="IKeyMode"/>.
        /// </summary>
        public bool IsVisible
        {
            get { return _model.Visible; }
            set
            {
                _model.Visible = value;
                OnPropertyChanged( "IsVisible" );
                OnPropertyChanged( "Visible" );
            }
        }

        /// <summary>
        /// Gets whether this LayoutKeyMode is a fallback or not.
        /// see <see cref="IKeyboardMode"/> for more explanations on the fallback concept
        /// </summary>
        public bool IsFallback
        {
            get
            {
                IKeyboardMode keyboardMode = Context.KeyboardContext.CurrentKeyboard.CurrentMode;
                return !keyboardMode.ContainsAll( _model.Mode ) || !_model.Mode.ContainsAll( keyboardMode );
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();
        }

    }
}