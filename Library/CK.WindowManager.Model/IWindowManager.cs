#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WindowManager.Model\IWindowManager.cs) is part of CiviKey. 
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
using System.Windows;
using CK.Plugin;

namespace CK.WindowManager.Model
{
    /// <summary>
    /// A <see cref="IWindowElement"/> can register itself into the window manager. 
    /// The window manager knows location and size of each registered <see cref="IWindowElement"/> and provides
    /// events about their state (when they moved, they are resized, hidden or restored).
    /// </summary>
    public interface IWindowManager : IDynamicService
    {
        /// <summary>
        /// Gets all regsitered <see cref="IWindowElement"/>.
        /// </summary>
        IReadOnlyList<IWindowElement> WindowElements { get; }

        /// <summary>
        /// Gets a <see cref="IWindowElement"/> by name. Null if no window element are registered with this name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IWindowElement GetByName( string name );

        Rect GetClientArea( IWindowElement e );

        /// <summary>
        /// Registers the given window element
        /// </summary>
        /// <param name="window"></param>
        void Register( IWindowElement windowElement );

        /// <summary>
        /// Unregister the given window element
        /// </summary>
        /// <param name="window"></param>
        void Unregister( IWindowElement windowElement );

        /// <summary>
        /// Moves the given window element top the top and left.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="top"></param>
        /// <param name="left"></param>
        IManualInteractionResult Move( IWindowElement window, double top, double left );

        /// <summary>
        /// Resizes the given window element to the width and height
        /// </summary>
        /// <param name="window"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        IManualInteractionResult Resize( IWindowElement window, double width, double height );

        void MinimizeAllWindows();

        void RestoreAllWindows();

        /// <summary>
        /// Minimizes the host.
        /// </summary>
        void ToggleHostMinimized();

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> got the focus.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementEventArgs> WindowGotFocus;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is registered.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementEventArgs> Registered;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is unregistered.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementEventArgs> Unregistered;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is hidden.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementEventArgs> WindowMinimized;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is restored.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementEventArgs> WindowRestored;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/>  is moved.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementLocationEventArgs> WindowMoved;

        /// <summary>
        /// Raised when a <see cref="IWindowElement"/> is resized.
        /// Called on the main dispatcher.
        /// </summary>
        event EventHandler<WindowElementResizeEventArgs> WindowResized;

    }
}
