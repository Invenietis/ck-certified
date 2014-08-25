#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\HelpServices\IHelpViewerService.cs) is part of CiviKey. 
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

using CK.Core;
using CK.Plugin;

namespace Help.Services
{
    public interface IHelpViewerService : IDynamicService
    {
        /// <summary>
        /// Open the default web browser to the plugin's help
        /// </summary>
        /// <param name="pluginName">The plugin name in order to find the help</param>
        /// <param name="force">Set force to true to open the web browser even is the content was not found</param>
        /// <returns>True if the content was found, false otherwise</returns>
        bool ShowHelpFor( IVersionedUniqueId pluginName, bool force = false );
    }
}
