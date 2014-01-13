﻿#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\Keyboard\CK.Keyboard.Model\Events\ZoneEventArgs.cs) is part of CiviKey. 
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
    /// Defines a keyboard zone event: gives access to the <see cref="IContext"/>, the <see cref="IKeyboard">keyboard</see> 
    /// and <see cref="IZone">zone</see> that is the subject of the event.
    /// </summary>
    public class ZoneEventArgs : KeyboardEventArgs
    {
        /// <summary>
        /// Gets the zone.
        /// </summary>
        public IZone Zone { get; private set; }

        public ZoneEventArgs( IZone l )
            : base( l.Keyboard )
        {
            Zone = l;
        }
    }

}
