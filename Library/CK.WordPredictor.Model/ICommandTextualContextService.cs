#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WordPredictor.Model\ICommandTextualContextService.cs) is part of CiviKey. 
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
using CK.Plugin;

namespace CK.WordPredictor.Model
{
    /// <summary>
    /// Sends the textual context service. 
    /// This is typically an action triggered by the user.
    /// </summary>
    public interface ICommandTextualContextService : IDynamicService
    {
        /// <summary>
        /// This event is raised when the <see cref="ITextualContextService"/> has been clear.
        /// </summary>
        event EventHandler TextualContextClear;

        /// <summary>
        /// Raises the <see cref="TextualContextClear"/> event
        /// </summary>
        void ClearTextualContext();
    }
}
