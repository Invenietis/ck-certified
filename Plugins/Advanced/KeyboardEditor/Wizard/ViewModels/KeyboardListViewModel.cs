#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ViewModels\KeyboardListViewModel.cs) is part of CiviKey. 
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
using System.Windows.Input;
using CK.Keyboard.Model;
using CK.Windows;
using CK.WPF.Wizard;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public class KeyboardListViewModel : HelpAwareWizardPage
    {
        public IList<KeyboardViewModel> KeyboardVms { get; set; }
        internal KeyboardViewModel _selectedKeyboard;
        List<IKeyboard> _keyboards;
        ICommand _selectionCommand;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        /// <param name="model">The keyboard to create or modify</param>
        public KeyboardListViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IEnumerable<IKeyboard> model )
            : base( root, wizardManager, false )
        {
            KeyboardVms = new List<KeyboardViewModel>();
            _keyboards = new List<IKeyboard>( model );
            foreach( var keyboard in model )
            {
                //temporary
                if( root.KeyboardContext.Service.CurrentKeyboard != keyboard && keyboard.Name != "Prediction" )
                    KeyboardVms.Add( new KeyboardViewModel( keyboard ) );
            }

            HideNext = true;
            Title = R.KeyboardListStepTitle;
            Description = R.KeyboardListStepDesc;
        }

        public override bool CheckCanGoFurther()
        {
            return Next != null && _selectedKeyboard != null;
        }

        public ICommand SelectionCommand
        {
            get
            {
                if( _selectionCommand == null ) _selectionCommand = new SimpleCommand<KeyboardViewModel>( ( k ) =>
                {
                    if( _selectedKeyboard != null )
                        _selectedKeyboard.IsSelected = false;

                    //The clicked keyboard is now the selected one.
                    k.IsSelected = true;
                    _selectedKeyboard = k;

                    OnKeyboardSelected( k );

                } );

                return _selectionCommand;
            }
        }

        public virtual void OnKeyboardSelected( KeyboardViewModel selectedKeyboardViewModel )
        {
        }
    }
}
