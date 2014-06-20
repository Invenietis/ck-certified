#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\KeyScroller\ScrollingStrategies\Tools\ITreeWalker.cs) is part of CiviKey. 
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
using CK.Core;
using HighlightModel;

namespace Scroller
{
    public interface ITreeWalker
    {
        /// <summary>
        /// Get the current parents stack
        /// </summary>
        Stack<IHighlightableElement> Parents { get; }

        /// <summary>
        /// Get the sibblings of the current element
        /// </summary>
        ICKReadOnlyList<IHighlightableElement> Sibblings { get; }

        /// <summary>
        /// Get the current element of the walker
        /// </summary>
        IHighlightableElement Current { get; }

        /// <summary>
        /// Move the walker to the next sibbling.
        /// </summary>
        /// <returns>false if there no next sibbling</returns>
        bool MoveNext();

        /// <summary>
        /// Move the walker to the first sibbling
        /// </summary>
        /// <returns></returns>
        void MoveFirst();

        /// <summary>
        /// Move the walker to the last sibbling
        /// </summary>
        /// <returns></returns>
        void MoveLast();

        /// <summary>
        /// Move the walker to the first child
        /// </summary>
        /// <returns>false if there no children</returns>
        bool EnterChild();

        /// <summary>
        /// Move the walker to the parent
        /// </summary>
        /// <returns>false if there no parent</returns>
        bool UpToParent();

        /// <summary>
        /// Move the walker to the given element
        /// </summary>
        /// <param name="element"></param>
        void GoTo(IHighlightableElement element);

        /// <summary>
        /// Move the walker to the absolute root
        /// </summary>
        void GoToAbsoluteRoot();

        /// <summary>
        /// move the cursor to the relative root (the root modul)
        /// </summary>
        void GoToRelativeRoot();
    }
}
