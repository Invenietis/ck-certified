#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ViewModels\SavingStepViewModel.cs) is part of CiviKey. 
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
using CK.Keyboard.Model;
using CK.Windows;
using CK.WPF.Wizard;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public class SavingStepViewModel : HelpAwareWizardPage
    {
        /// <summary>
        /// Gets the list of <see cref="WizardButtonViewModel"/> of this <see cref="WizardPage"/>
        /// </summary>
        public IList<WizardButtonViewModel> Buttons { get; private set; }
        SimpleCommand<WizardButtonViewModel> _command;
        WizardButtonViewModel _selected;
        IKeyboard _keyboardToSave;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="root">The ContextEditor that is the root of this wizard</param>
        /// <param name="wizardManager">The wizardManager</param>
        /// <param name="keyboard">The modified keyboard to save</param>
        public SavingStepViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IKeyboard keyboard )
            : base( root, wizardManager, false )
        {
            _keyboardToSave = keyboard;

            Buttons = new List<WizardButtonViewModel>();
            HideNext = true;
            Buttons.Add( new WizardButtonViewModel( R.SavingStepSaveTitle, R.SavingStepSaveDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/save-keyboard.png", SaveOnEditedKeyboard ) );
            Buttons.Add( new WizardButtonViewModel( R.SavingStepSaveAsTitle, R.SavingStepSaveAsDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/save-as.png", SaveUnderOtherKeyboard ) );
            Buttons.Add( new WizardButtonViewModel( R.SavingStepCancelAndQuitTitle, R.SavingStepCancelAndQuitDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/cancel-quit.png", CancelAndQuit ) );
            Buttons.Add( new WizardButtonViewModel( R.SavingStepCancelAndRestartTitle, R.SavingStepCancelAndRestartDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/cancel-restart.png", CancelAndRestart ) );

            Title = R.SavingStepTitle;
            Description = String.Format( R.SavingStepDesc, keyboard.Name );
        }

        /// <summary>
        /// Gets the command called when the user clicks on a WizardButtonViewModel
        /// </summary>
        public SimpleCommand<WizardButtonViewModel> ButtonCommand
        {
            get
            {
                if( _command == null ) _command = new SimpleCommand<WizardButtonViewModel>( ( k ) =>
                {
                    if( _selected != null )
                        _selected.IsSelected = false;

                    _selected = k;
                    k.IsSelected = true;
                    NotifyOfPropertyChange( () => CanGoFurther );
                } );
                return _command;
            }
        }

        /// <summary>
        /// Goes to the next page.
        /// Not doing anything means keeping the modifications done on the keyboard on which they have been done.
        /// </summary>
        private void SaveOnEditedKeyboard()
        {
            Root.Save();
            Next = new EndingStepViewModel( Root, WizardManager );
            WizardManager.GoFurther();
        }

        /// <summary>
        /// Saves the modifications done on another keyboard and restores the current one.
        /// Note that in reality, the current keyboard holds the modifications, so we will rename it, 
        /// and restore the backup of the previous state of the keyboard on a new keyboard that we'll create with the name it previously had.
        /// </summary>
        private void SaveUnderOtherKeyboard()
        {
            Next = new SaveAsStepViewModel( Root, WizardManager, _keyboardToSave );
            WizardManager.GoFurther();
        }

        private void CancelAndQuit()
        {
            Root.CancelModifications();
            WizardManager.Close();
        }

        private void CancelAndRestart()
        {
            Root.CancelModifications();
            WizardManager.Restart();
        }

        /// <summary>
        /// Checks that a keyboard has been selected before enabling going to the next step.
        /// </summary>
        /// <returns>true if a keyboard has been selected, false otherwise</returns>
        public override bool CheckCanGoFurther()
        {
            return Buttons.Any( ( b ) => b.IsSelected );
        }
    }
}
