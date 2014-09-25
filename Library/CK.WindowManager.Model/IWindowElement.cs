#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WindowManager.Model\IWindowElement.cs) is part of CiviKey. 
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
using System.Windows;
using Host.Services;

namespace CK.WindowManager.Model
{
    public interface IWindowElement : IDisposable
    {
        /// <summary>
        /// Raised when the window element is focused.
        /// </summary>
        event EventHandler GotFocus;

        /// <summary>
        /// Raised when the window element location changed.
        /// </summary>
        event EventHandler LocationChanged;

        /// <summary>
        /// Raised when the window element size changed.
        /// </summary>
        event EventHandler SizeChanged;

        /// <summary>
        /// Raised when the window element is hidden.
        /// </summary>
        event EventHandler Minimized;

        /// <summary>
        /// Raised when the window element is restored (from hidden state for ex).
        /// </summary>
        event EventHandler Restored;

        /// <summary>
        /// Gets the name of the window element.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets top edge, in relation to the desktop, in logical units (1/96").
        /// If called from another thread and in order to get this info without fearing deadlocks, try <see cref="IWindowManager.GetClientArea"/> 
        /// </summary>
        double Top { get; }

        /// <summary>
        /// Gets the left edge, in relation to the desktop, in logical units (1/96").
        /// If called from another thread and in order to get this info without fearing deadlocks, try <see cref="IWindowManager.GetClientArea"/> 
        /// </summary>
        double Left { get; }

        /// <summary>
        /// Gets the width, in device-independent units (1/96").
        /// If called from another thread and in order to get this info without fearing deadlocks, try <see cref="IWindowManager.GetClientArea"/> 
        /// </summary>
        double Width { get; }

        /// <summary>
        /// Gets the height, in device-independent units (1/96").
        /// If called from another thread and in order to get this info without fearing deadlocks, try <see cref="IWindowManager.GetClientArea"/> 
        /// </summary>
        double Height { get; }

        /// <summary>
        /// Moves the window element at the given top and left in relation to the desktop.
        /// </summary>
        /// <param name="top">The top in logical units (1/96")</param>
        /// <param name="left">The left in logical units (1/96")</param>
        void Move( double top, double left );

        /// <summary>
        /// Resizes the window element to the given width and height
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void Resize( double width, double height );

        /// <summary>
        /// Hides the window element
        /// </summary>
        void Hide();

        /// <summary>
        /// Shows the window element
        /// </summary>
        void Show();

        /// <summary>
        /// Closes the window element
        /// </summary>
        void Close();

        /// <summary>
        /// Minimizes the window element
        /// </summary>
        void Minimize();

        /// <summary>
        /// Restores the window element
        /// </summary>
        void Restore();

        void ToggleHostMinimized( IHostManipulator manipulator );

        Window Window { get; }
    }

}
