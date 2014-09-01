#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\HighlightModel\IExtensionHighlightableElement.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightModel
{
    public interface IExtensibleHighlightableElement : IHighlightableElement
    {
        /// <summary>
        /// Allows the addition in PreChildren or PostChildren collections
        /// </summary>
        /// <param name="position"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        bool RegisterElementAt( ChildPosition position, IHighlightableElement child );

        /// <summary>
        /// Allows the deletion in PreChildren or PostChildren collections
        /// </summary>
        /// <param name="position"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        bool UnregisterElement( ChildPosition position, IHighlightableElement element );

        /// <summary>
        /// This is the name that identifies the IExtensibleHighlightableElement.
        /// </summary>
        string ElementName { get; }

        /// <summary>
        /// The list of virtual children with the <see cref="ChildPosition"/>.Pre. In front of the list of Children
        /// </summary>
        IReadOnlyList<IHighlightableElement> PreChildren { get; }

        /// <summary>
        /// The list of virtual children with the <see cref="ChildPosition"/>.Post. Behind the list of Children
        /// </summary>
        IReadOnlyList<IHighlightableElement> PostChildren { get; }
    }
}