#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\Keyboard\CK.Keyboard.Model\Events\KeyModeModeSwappedEventArgs.cs) is part of CiviKey. 
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

namespace CK.Keyboard.Model
{
    /// <summary>
    /// Defines the argument of event that fires when two actual keys swap their modes.
    /// This happens only during the edition of a keyboard: the event is raised only 
    /// by <see cref="IKeyMode.SwapModes"/> method.
    /// </summary>
    public class KeyModeModeSwappedEventArgs : KeyModeModeChangedEventArgs
    {
        /// <summary>
        /// Gets the actual key that hold the <see cref="KeyModeModeChangedEventArgs.PreviousMode">previous mode</see>.
        /// </summary>
        public IKeyMode SwappedKey { get; private set; }

        public KeyModeModeSwappedEventArgs( IKeyMode key, IKeyMode swappedKey )
            : base( key, swappedKey.Mode )
        {
            SwappedKey = swappedKey;
        }
    }
}
