#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\Keyboard\KeyboardCollection.Active.cs) is part of CiviKey. 
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
using System.Collections;
using CK.Keyboard.Model;
using System.Linq;

namespace CK.Keyboard
{
    sealed partial class KeyboardCollection : IKeyboardCollection
    {
        public event EventHandler<KeyboardEventArgs> KeyboardActivated;

        public event EventHandler<KeyboardEventArgs> KeyboardDeactivated;

        ICollection _col;

        public ICollection<IKeyboard> Actives
        {
            get { return _keyboards.Values.Where( x => x.IsActive ).ToArray(); }
        }

        internal void ChangeActivateState( Keyboard keyboard, ref bool isActive, bool newState )
        {
            if( newState == true )
            {
                isActive = true;
                if( KeyboardActivated != null )
                    KeyboardActivated( this, new KeyboardEventArgs( keyboard ) );
            }
            if( newState == false )
            {
                isActive = false;
                if( KeyboardDeactivated != null )
                    KeyboardDeactivated( this, new KeyboardEventArgs( keyboard ) );
            }
        }
    }
}
