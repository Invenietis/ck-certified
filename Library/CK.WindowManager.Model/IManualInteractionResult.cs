#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WindowManager.Model\IManualInteractionResult.cs) is part of CiviKey. 
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

namespace CK.WindowManager.Model
{
    /// <summary>
    /// Defines what can be bone after a manual interaction on the WindowManager.
    /// </summary>
    public interface IManualInteractionResult : CK.Core.IFluentInterface
    {
        /// <summary>
        /// Does not propagate the event. This is the default behavior.
        /// </summary>
        void Silent();
        /// <summary>
        /// Broadcast the event resulting of the manual interaction.
        /// </summary>
        void Broadcast();
    }
}
