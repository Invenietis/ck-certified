#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ViewModels\ElementViewModels\CheckBoxImportKeyboardViewModel.cs) is part of CiviKey. 
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
using System.Text;
using System.Windows.Input;
using CK.WPF.ViewModel;

namespace KeyboardEditor.Wizard.ViewModels
{
    public class CheckBoxImportKeyboardViewModel : VMBase
    {
        string _keyboardName;
        bool _isSelected;
        bool _alreadyExist;

        public CheckBoxImportKeyboardViewModel( string keyboardName, bool alreadyExist )
        {
            _keyboardName = keyboardName;
            _alreadyExist = alreadyExist;
            _isSelected = false;
            CheckCommand = new VMCommand( () => IsSelected = (IsSelected) ? false : true );
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if( value != _isSelected )
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AlreadyExist 
        {
            get { return _alreadyExist; }
            set
            {
                if( value != _alreadyExist )
                {
                    _alreadyExist = value;
                    OnPropertyChanged();
                }
            } 
        }

        public string KeyboardName
        {
            get { return _keyboardName; }
        }

        public ICommand CheckCommand
        {
            get;
            private set;
        }
    }
}
