#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WordPredictor.Model\IKeyboardContextPredictionFactory.cs) is part of CiviKey. 
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
using CK.Keyboard.Model;

namespace CK.WordPredictor.Model
{
    public interface IKeyboardContextPredictionFactory
    {
        /// <summary>
        /// Gets the name of the underlying prediction zone
        /// </summary>
        string PredictionKeyboardAndZoneName { get; }

        /// <summary>
        /// Creates the prediction zone with the number of keys. Pass 0 to create an empty zone.
        /// This will raises the <see cref="PredictionZoneCreated"/> after the zone has been totally created and fill of keys.
        /// If the keyboard is not supported, the zone is not created. 
        /// </summary>
        /// <param name="keyboard">The keyboard in which the prediction zone must be created</param>
        /// <param name="count">The number of keys to add to the prediction zone</param>
        /// <returns>The prediction zone model <see cref="IZone"/>.</returns>
        IZone CreatePredictionZone( IKeyboard keyboard, int count );

        /// <summary>
        /// Removes the prediction zone in the given keyboard.
        /// </summary>
        /// <param name="keyboard"></param>
        void RemovePredictionZone( IKeyboard keyboard );

        /// <summary>
        /// Cretes a prediction key at the given index in the given zone.
        /// </summary>
        /// <remarks>
        /// The zone can be any zone, and not necessarily the prediction zone.
        /// </remarks>
        /// <param name="zone"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        IKey CreatePredictionKey( IZone zone, int index );

        /// <summary>
        /// This event is raised when the prediction zone is created.
        /// </summary>
        event EventHandler<ZoneEventArgs> PredictionZoneCreated;
    }
}
