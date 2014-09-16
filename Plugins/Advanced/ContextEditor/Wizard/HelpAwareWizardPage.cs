#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\HelpAwareWizardPage.cs) is part of CiviKey. 
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

using System.Windows.Input;
using CK.WPF.Wizard;

namespace KeyboardEditor
{
    public abstract class HelpAwareWizardPage : WizardPage
    {
        internal IKeyboardEditorRoot Root { get; private set; }

        public HelpAwareWizardPage( IKeyboardEditorRoot root, WizardManager wizardManager, WizardPage next, bool isLastStep, string title = "" )
            : base( wizardManager, next, title )
        {
            Root = root;
        }

        public HelpAwareWizardPage( IKeyboardEditorRoot root, WizardManager wizardManager, WizardPage next, string title = "" )
            : this( root, wizardManager, next, false, title )
        {
        }

        public HelpAwareWizardPage( IKeyboardEditorRoot root, WizardManager wizardManager, bool isLastStep, string title = "" )
            : this( root, wizardManager, null, isLastStep, title )
        {
        }

        ICommand _showHelpCommand;
        public ICommand ShowHelpCommand
        {
            get
            {
                if( _showHelpCommand == null )
                {
                    _showHelpCommand = new CK.Windows.App.VMCommand( () => Root.ShowHelp() );
                }
                return _showHelpCommand;
            }
        }
    }
}
