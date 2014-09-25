#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WindowManager.Model\IBinding.cs) is part of CiviKey. 
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

namespace CK.WindowManager.Model
{
    [Flags]
    public enum BindingPosition
    {
        None = 0,
        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8
    }
    /// <summary>
    /// Represents a binding between two <see cref="IWindowElement"/>.
    /// </summary>
    public interface IBinding
    {
        /// <summary>
        /// Represents the position of the Master window relative to Slave the  window.
        /// <remarks>
        /// Target
        /// Origin
        /// > Position: Bottom
        /// 
        /// Origin - Target > Position: Left
        /// 
        /// Origin
        /// Target
        /// > Position: Top.
        /// 
        /// Target - Origin > Position: Right
        /// </remarks>
        /// </summary>
        BindingPosition Position { get; }

        /// <summary>
        /// The target window element
        /// </summary>
        IWindowElement Target { get; }

        /// <summary>
        /// The origin window element (or current window)
        /// </summary>
        IWindowElement Origin { get; }
    }

    public interface ISpatialBinding
    {
        /// <summary>
        /// Gets the reference window
        /// </summary>
        IWindowElement Window { get; }

        /// <summary>
        /// Gets the left binding of the reference window
        /// </summary>
        ISpatialBinding Left { get; }

        /// <summary>
        /// Gets the right binding of the reference window
        /// </summary>
        ISpatialBinding Right { get; }

        /// <summary>
        /// Gets the bottom binding of the reference window
        /// </summary>
        ISpatialBinding Bottom { get; }

        /// <summary>
        /// Gets the top binding of the reference window
        /// </summary>
        ISpatialBinding Top { get; }
    }

    //public interface ISpatialBindingWithButtonElement
    //{
    //    /// <summary>
    //    /// Gets the ISpatialBinding
    //    /// </summary>
    //    ISpatialBinding SpatialBinding { get; }

    //    /// <summary>
    //    /// Gets the associated UnbindButton
    //    /// </summary>
    //    IWindowElement UnbindButton { get; }
    //}
}
