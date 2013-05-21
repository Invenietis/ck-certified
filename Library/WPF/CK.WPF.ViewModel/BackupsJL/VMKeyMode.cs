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
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace CK.WPF.ViewModel
{
    public abstract class VMKeyMode<TC, TB, TZ, TK, TKM, TLKM> : VMContextElement<TC, TB, TZ, TK, TKM, TLKM>
        where TC : VMContext<TC, TB, TZ, TK, TKM, TLKM>
        where TB : VMKeyboard<TC, TB, TZ, TK, TKM, TLKM>
        where TZ : VMZone<TC, TB, TZ, TK, TKM, TLKM>
        where TK : VMKey<TC, TB, TZ, TK, TKM, TLKM>
        where TKM : VMKeyMode<TC, TB, TZ, TK, TKM, TLKM>
        where TLKM : VMLayoutKeyMode<TC, TB, TZ, TK, TKM, TLKM>
    {
        TC _context;
        IKeyMode _model;
        public VMKeyMode( TC context, IKeyMode keyMode )
            : base( context )
        {
            _context = context;
            _model = keyMode;
        }

        public void TriggerPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );
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

        ///Gets the UpLabel of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public string UpLabel
        {
            get { return _model.UpLabel; }
            set { _model.UpLabel = value; }
        }

        ///Gets the DownLabel of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public string DownLabel
        {
            get { return _model.DownLabel; }
            set { _model.DownLabel = value; }
        }

        ///Gets the Description of the underling <see cref="IKeyMode"/>
        public string Description
        {
            get { return _model.Description; }
            set { _model.Description = value; }
        }

        /// <summary>
        /// Gets a value indicating wether the current keymode is enabled or not.
        /// </summary>
        public bool Enabled { get { return _model.Enabled; } }

        protected override void OnDispose()
        {
            base.OnDispose();
        }

    }
}