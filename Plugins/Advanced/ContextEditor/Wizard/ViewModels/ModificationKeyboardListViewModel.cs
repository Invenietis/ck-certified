#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ViewModels\ModificationKeyboardListViewModel.cs) is part of CiviKey. 
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
using CK.WPF.Wizard;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public class ModificationKeyboardListViewModel : KeyboardListViewModel
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        /// <param name="model">The keyboard to create or modify</param>
        public ModificationKeyboardListViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IKeyboardCollection model )
            : base( root, wizardManager, model )
        {
            HideNext = true;
            Title = R.KeyboardListStepTitle;
            Description = R.KeyboardListStepDesc;
        }

        public override void OnKeyboardSelected( KeyboardViewModel selectedKeyboardViewModel )
        {
            //We update the Next property to give it the proper model.
            Next = new KeyboardProfileViewModel( Root, WizardManager, _selectedKeyboard.Keyboard );

            NotifyOfPropertyChange( () => IsLastStep );
            NotifyOfPropertyChange( () => CanGoFurther );

            WizardManager.GoFurther();
        }
    }
}
