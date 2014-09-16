#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ViewModels\DeletionKeyboardListViewModel.cs) is part of CiviKey. 
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

using System.Collections.Generic;
using CK.Keyboard.Model;
using CK.WPF.Wizard;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public class DeletionKeyboardListViewModel : KeyboardListViewModel
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        /// <param name="model">The keyboard to create or modify</param>
        public DeletionKeyboardListViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IEnumerable<IKeyboard> model )
            : base( root, wizardManager, model )
        {
            HideNext = false;
            Title = R.KeyboardDeletionListStepTitle;
            Description = R.KeyboardDeletionListStepDesc;
        }

        public override void OnKeyboardSelected( KeyboardViewModel selectedKeyboardViewModel )
        {
            Next = new EndingStepViewModel( Root, WizardManager );

            NotifyOfPropertyChange( () => IsLastStep );
            NotifyOfPropertyChange( () => CanGoFurther );
        }

        public override bool OnBeforeNext()
        {
            _selectedKeyboard.Keyboard.Destroy();
            return base.OnBeforeNext();
        }
    }
}
