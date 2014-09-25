#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ViewModels\ElementViewModels\SimpleKeyboardViewModel.cs) is part of CiviKey. 
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

using CK.Keyboard.Model;

namespace KeyboardEditor.ViewModels
{
    /// <summary>
    /// This class is the viewmodel that corresponds to the "Profile" of a keyboard.
    /// Corresponds to its name, width and height.
    /// </summary>
    public class SimpleKeyboardViewModel
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Ctro to create a SimpleKeyboardViewModel from a <see cref="IKeyboard"/>
        /// </summary>
        /// <param name="keyboard">The model</param>
        public SimpleKeyboardViewModel( IKeyboard keyboard )
        {
            Name = keyboard.Name;
            Height = keyboard.CurrentLayout.H;
            Width = keyboard.CurrentLayout.W;
        }

        /// <summary>
        /// Creates a default SimpleKeyboardViewModel
        /// </summary>
        public SimpleKeyboardViewModel()
        {
            Name = "Nouveau clavier";
            Height = 400;
            Width = 800;
        }
    }
}
