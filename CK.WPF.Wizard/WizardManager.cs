#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.WPF.Wizard\WizardManager.cs) is part of CiviKey. 
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

using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using CK.Windows;

namespace CK.WPF.Wizard
{
    /// <summary>
    /// This specialization of the StackConductor implements the "GoFurther" (Go to the next view) and "Restart" actions.
    /// </summary>
    public class WizardManager : NavigableConductor<WizardPage>
    {
        ViewAware _parent;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="parent">The ViewAware that is the root of the Wizard</param>
        public WizardManager( ViewAware parent )
        {
            _parent = parent;
        }

        /// <summary>
        /// Closes the hosting window
        /// </summary>
        public void Close()
        {
            Window w = _parent.GetView() as Window;
            if( w != null ) w.Close();
        }

        SimpleCommand _goFurtherCommand;
        SimpleCommand _closeCommand;
        /// <summary>
        /// Triggers <see cref="GoFurther()", which activates the next view/>
        /// </summary>
        public ICommand GoFurtherCommand
        {
            get
            {
                if( _goFurtherCommand == null ) _goFurtherCommand = new SimpleCommand( GoFurther );
                return _goFurtherCommand;
            }
        }

        SimpleCommand _goBackCommand;
        /// <summary>
        /// Triggers <see cref="GoBack()", which activates the previous view/>
        /// </summary>
        public ICommand GoBackCommand
        {
            get
            {
                if( _goBackCommand == null ) _goBackCommand = new SimpleCommand( GoBack );
                return _goBackCommand;
            }
        }

        /// <summary>
        /// Triggers <see cref="Close()", which closes the hosting window/>
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                if( _closeCommand == null ) _closeCommand = new SimpleCommand( Close );
                return _closeCommand;
            }
        }

        /// <summary>
        /// Resets the stack to start again at the beginning of the wizard.
        /// WizardPages are not re-created.
        /// </summary>
        public void Restart()
        {
            GoBackToRoot();
        }
    }
}
