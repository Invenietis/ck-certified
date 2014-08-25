#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\HighlightModel\IHighlightableElementController.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightModel
{
    public interface IHighlightableElementController
    {
        /// <summary>
        /// Allows the parent to override the <see cref="ActionType"/> of children
        /// </summary>
        /// <param name="element">The child</param>
        /// <param name="action">ActionType send by child</param>
        /// <returns>The ActionType send by parent</returns>
        ActionType PreviewChildAction(IHighlightableElement element, ActionType action);

        /// <summary>
        /// Inform that a parent has overridden an <see cref="ActionType"/>
        /// </summary>
        /// <param name="action"></param>
        void OnChildAction( ActionType action );

        /// <summary>
        /// Inform that the element was unregistered in the tree.
        /// </summary>
        void OnUnregisterTree();
    }
}
