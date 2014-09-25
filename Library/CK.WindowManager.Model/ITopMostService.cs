#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WindowManager.Model\ITopMostService.cs) is part of CiviKey. 
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

using System.Windows;
using CK.Plugin;

namespace CK.WindowManager.Model
{
    /// <summary>
    /// Creates a system of "z-index" relative to multiple windows in Topmost
    /// </summary>
    public interface ITopMostService : IDynamicService
    {
        /// <summary>
        /// Registers a window in the display hierarchy.
        /// </summary>
        /// <param name="levelName">Index of the Window</param>
        /// <param name="window">Window to register</param>
        /// <returns>Return false, if the window already exist or if int.TryParse fail, otherwise true</returns>
        bool RegisterTopMostElement( string levelName, Window window );

        /// <summary>
        /// Unregister a window in the display hierachy.
        /// </summary>
        /// <param name="window">Window to unregister</param>
        /// <returns>Return false, if the window isn't register</returns>
        bool UnregisterTopMostElement( Window window );

    }
}
