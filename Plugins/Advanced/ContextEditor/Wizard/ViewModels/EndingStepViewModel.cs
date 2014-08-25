#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ViewModels\EndingStepViewModel.cs) is part of CiviKey. 
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

using System.Collections.Generic;
using System.Linq;
using CK.Windows;
using CK.WPF.Wizard;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public class EndingStepViewModel : HelpAwareWizardPage
    {
        public IList<WizardButtonViewModel> Buttons { get; set; }

        public override bool CheckCanGoFurther()
        {
            return Buttons.Any( ( b ) => b.IsSelected );
        }

        public EndingStepViewModel( IKeyboardEditorRoot root, WizardManager wizardManager )
            : base( root, wizardManager, true )
        {
            Buttons = new List<WizardButtonViewModel>();
            HideNext = true;
            HideBack = true;

            Buttons.Add( new WizardButtonViewModel( R.Quit, R.EndingStepQuitDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/exit.png", CloseWizard ) );
            Buttons.Add( new WizardButtonViewModel( R.StartOver, R.EndingStepStartOverDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/restart.png", RestartWizard ) );

            Title = R.EndingStepTitle;
            Description = R.EndingStepDesc;
        }

        SimpleCommand<WizardButtonViewModel> _command;
        public SimpleCommand<WizardButtonViewModel> ButtonCommand
        {
            get
            {
                if( _command == null ) _command = new SimpleCommand<WizardButtonViewModel>( ( k ) =>
                {
                    k.IsSelected = true;
                } );
                return _command;
            }
        }

        public void CloseWizard()
        {
            Root.EnsureBackupIsClean();
            WizardManager.Close();
        }

        public void RestartWizard()
        {
            Root.EnsureBackupIsClean();
            WizardManager.Restart();
        }
    }
}
