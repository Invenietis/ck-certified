#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\TextTemplate\IText.cs) is part of CiviKey. 
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

using HighlightModel;

namespace TextTemplate
{
    public interface IText : IHighlightableElement, IHighlightable
    {
        /// <summary>
        /// True if the property Text is writeable
        /// </summary>
        bool IsEditable { get; }

        /// <summary>
        /// Return the text. If IsEditable is set to false, all attempts to set Text property should throw an exception.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// The text placeholder
        /// </summary>
        string Placeholder { get; set; }
    }

    public interface IHighlightable
    {
        /// <summary>
        /// Wheter the IText is highlight or not
        /// </summary>
        bool IsHighlighted { get; set; }

        /// <summary>
        /// Weather if the text is selected or not
        /// </summary>
        bool IsSelected { get; set; }
    }
}
