#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\Logs\Model\ILogServiceConfig.cs) is part of CiviKey. 
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

using CK.Core;

namespace CK.Plugins.ObjectExplorer
{
    public interface ILogServiceConfig
    {
        /// <summary>
        /// Full name of the service (type name with its namespace).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets a boolean that enables/disables any log for this service.
        /// </summary>
        bool DoLog { get; }


        ICKReadOnlyCollection<ILogMethodConfig> Methods { get; }

        ICKReadOnlyCollection<ILogEventConfig> Events { get; }
        
        ICKReadOnlyCollection<ILogPropertyConfig> Properties { get; }
      
    }
}
