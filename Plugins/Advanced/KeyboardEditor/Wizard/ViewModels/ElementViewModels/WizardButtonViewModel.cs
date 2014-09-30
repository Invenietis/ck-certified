#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ViewModels\ElementViewModels\WizardButtonViewModel.cs) is part of CiviKey. 
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

using Caliburn.Micro;

namespace KeyboardEditor.ViewModels
{
    /// <summary>
    /// A button to display on a wizard.
    /// Has a <see cref="IsSelected"/> state and an action executed when <see cref="IsSelected"/> is set to true
    /// Has a main Label and a description
    /// </summary>
    public class WizardButtonViewModel : PropertyChangedBase
    {
        bool _isSelected;

        /// <summary>
        /// Gets the <see cref="System.Action"/> triggered when the button is clicked.
        /// </summary>
        public System.Action OnClickAction { get; private set; }

        /// <summary>
        /// Gets the desciption od the button
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets th emain label of the button
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Gets or sets whether the button is selected or not.
        /// setting to true will trigger the <see cref="OnClickAction"/> of the button.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                NotifyOfPropertyChange( () => IsSelected );

                if( value ) OnClickAction();
            }
        }

        /// <summary>
        /// Gets the path to an image bound to this button.
        /// </summary>
        public string ImagePath { get; private set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="label">The main label of the button</param>
        /// <param name="description">The description of the button</param>
        /// <param name="imagePath">The path to an image (optional) </param>
        /// <param name="onClickAction">The action called when the button is selected</param>
        public WizardButtonViewModel( string label, string description, string imagePath, System.Action onClickAction )
        {
            OnClickAction = onClickAction;
            Description = description;
            Label = label;
            ImagePath = imagePath;
        }
    }
}
