#region LGPL License
/*----------------------------------------------------------------------------
* This file (CVKServices\System\ICVKCommonTimer.cs) is part of CiviKey. 
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
* Copyright © 2007-2009, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using CK.Plugin;

namespace CommonServices
{
    public interface ICommonTimer : IDynamicService
    {
        /// <summary>
        /// Fires at the <see cref="Interval"/> specified (milli seconds).
        /// </summary>
        event EventHandler Tick;

        /// <summary>
        /// Fires when the interval configuration changes.
        /// </summary>
        event EventHandler IntervalChanged;

        /// <summary>
        /// Gets or sets the current delay between ticks in milliseconds of the common timer.
        /// This configuration defaults to 500ms and is stored in the system configuration.
        /// </summary>
        int Interval { get; set; }
    }
}
