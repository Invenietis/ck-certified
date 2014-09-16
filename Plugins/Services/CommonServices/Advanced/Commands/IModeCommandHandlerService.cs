#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\CommonServices\Advanced\Commands\IModeCommandHandlerService.cs) is part of CiviKey. 
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
using CK.Keyboard.Model;
using CK.Plugin;

namespace CommonServices
{
    /// <summary>
    /// Service which allow you to change the current keyboard mode and listen all mode changes done from this command handler.
    /// </summary>
    public interface IModeCommandHandlerService : IDynamicService
    {
        /// <summary>
        /// Changes the current mode of the current keyboard.
        /// </summary>
        /// <param name="mode">String which represent the new mode. It can be a composite mode.</param>
        void ChangeMode( string mode );

        /// <summary>
        /// Add the given mode to the current keyboard mode.
        /// </summary>
        /// <param name="mode"></param>
        void Add( string mode );

        /// <summary>
        /// Remove the given mode to the current keyboard mode.
        /// </summary>
        /// <param name="mode"></param>
        void Remove( string mode );

        /// <summary>
        /// Toogle the given mode with the current keyboard mode.
        /// </summary>
        /// <param name="mode"></param>
        void Toggle( string mode );

        /// <summary>
        /// Raised when the mode is changed by this service.
        /// </summary>
        event EventHandler<ModeChangedEventArgs> ModeChangedByCommandHandler;
    }

    public class ModeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// String which represent the new mode.
        /// </summary>
        public readonly IKeyboardMode Mode;

        public ModeChangedEventArgs( IKeyboardMode mode ) { Mode = mode; }

    }
}