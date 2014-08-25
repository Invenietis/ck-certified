#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\Keyboard\CK.Keyboard.Model\IZoneCollection.cs) is part of CiviKey. 
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
using CK.Core;

namespace CK.Keyboard.Model
{
    /// <summary>
    /// Collection containing all the zones corresponding to a keyboard.
    /// </summary>
    public interface IZoneCollection : ICKReadOnlyCollection<IZone>
    {
        /// <summary>
        /// Gets the <see cref="IKeyboardContext"/> that hold the <see cref="Keyboard"/>. 
        /// </summary>
        IKeyboardContext Context { get; }

        /// <summary>
        /// Gets the <see cref="IKeyboard"/> that hold this <see cref="IZoneCollection"/>. 
        /// </summary>
        IKeyboard Keyboard { get; }

        /// <summary>
        /// Gets one of the <see cref="IZone"/> by its name.
        /// </summary>
        /// <param name="name">Name of the zone to find.</param>
        /// <returns>The <see cref="IZone"/> object or null if not found.</returns>
        IZone this[string name] { get; }

        /// <summary>
        /// Gets one of the <see cref="IZone"/> by its index.
        /// </summary>
        /// <param name="name">Index of the zone to find.</param>
        /// <returns>The <see cref="IZone"/> object or throws an exception if not found.</returns>
        IZone this[int index] { get; }
        
        /// <summary>
        /// This method creates and adds a <see cref="IZone"/> in this collection.
        /// The <see cref="ZoneCreated"/> event is raised.
        /// </summary>
        /// <param name="name">The proposed zone name.</param>
        /// <returns>The new zone.</returns>
        /// <remarks>
        /// Note that its <see cref="IZone.Name"/> may be different than <paramref name="name"/> if a zone already exists
        /// with the proposed name.
        /// The zone will be created with an index equal to the zone list count.
        /// </remarks>
        IZone Create( string name );

        /// <summary>
        /// This method creates and adds a <see cref="IZone"/> in this collection.
        /// The <see cref="ZoneCreated"/> event is raised.
        /// </summary>
        /// <param name="name">The proposed zone name.</param>
        /// <param name="index">The proposed index. Set -1 to add at the last index.</param>
        /// <returns>The new zone.</returns>
        /// <remarks>
        /// Note that its <see cref="IZone.Name"/> may be different than <paramref name="name"/> if a zone already exists
        /// with the proposed name.
        /// If the index set is superior to the zone count or is equal to -1, the zone will be created at the index right next to the last zone.
        /// </remarks>
        IZone Create( string name, int index );

        /// <summary>
        /// Gets the default zone: its <see cref="IZone.Name"/> is an empty string and it 
        /// can not be <see cref="IZone.Destroy">destroyed</see> nor <see cref="IZone.Rename">renamed</see>.
        /// </summary>
        IZone Default { get; }

        /// <summary>
        /// Fires whenever a <see cref="IZone"/> has been created.
        /// </summary>
        event EventHandler<ZoneEventArgs> ZoneCreated;

        /// <summary>
        /// Fires whenever a <see cref="IZone"/> has been destroyed.
        /// </summary>
        event EventHandler<ZoneEventArgs> ZoneDestroyed;


        /// <summary>
        /// Fires whenever one of the zone contained in this collection has been renamed.
        /// </summary>
        event EventHandler<ZoneEventArgs> ZoneRenamed;


        /// <summary>
        /// Fires whenever one of the zone contained in this collection has been moved.
        /// </summary>
        event EventHandler<ZoneEventArgs> ZoneMoved;



    }
}
