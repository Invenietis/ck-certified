#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ViewModels\ExportKeyboardViewModel.cs) is part of CiviKey. 
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
using System.Windows;
using System.Windows.Input;
using CK.Keyboard.Model;
using CK.WPF.ViewModel;

namespace KeyboardEditor.Wizard.ViewModels
{
    public class ExportKeyboardViewModel : VMBase
    {
        readonly IKeyboardCollection _keyboards;
        readonly ExportKeyboard _owner;

        List<CheckBoxExportKeyboardViewModel> _checkBoxs;
        string _fileName;
        bool _canExecute;

        public ExportKeyboardViewModel( ExportKeyboard owner, IKeyboardCollection keyboards )
        {
            _checkBoxs = new List<CheckBoxExportKeyboardViewModel>();
            _owner = owner;
            _keyboards = keyboards;
            _canExecute = true;
            CreateCheckBox();
            SaveCommand = new VMCommand( ShowSaveWindow, o => CanExecute );
        }

        public bool CanExecute
        {
            get { return _canExecute; }
            set
            {
                if( value != _canExecute )
                {
                    _canExecute = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<CheckBoxExportKeyboardViewModel> CheckBoxs
        {
            get { return _checkBoxs; }
        }

        void CreateCheckBox()
        { 
            foreach( var k in _keyboards )
            {
                if( k.Keyboard.Name != "Prediction") //TEMPORARY
                    _checkBoxs.Add( new CheckBoxExportKeyboardViewModel( k ) );
            }
        }

        public ICommand SaveCommand
        {
            get;
            internal set;
        }

        bool CheckSelectedKeyboards()
        {
            if( _checkBoxs.Any( cb => cb.IsSelected ) )
            {
                return true;
            }
            MessageBox.Show( "Aucun clavier n'est sélectionné.", "Information", System.Windows.MessageBoxButton.OK );
            return false;
        }


        void ShowSaveWindow()
        {
            CanExecute = false;

            if( CheckSelectedKeyboards() )
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "Keyboards"; // Default file name
                dlg.DefaultExt = ".xml"; // Default file extension
                dlg.Filter = "CiviKey keyboard (.xml)|*.xml"; // Filter files by extension

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process save file dialog box results
                if( result == true )
                {
                    SaveInXml( dlg.FileName );
                }
            }

            CanExecute = true;
        }

        void SaveInXml( string fileName )
        {
            List<IKeyboard> keyboardsToSerialize = new List<IKeyboard>();
            // Save document
            foreach( var cb in _checkBoxs )
            {
                if( cb.IsSelected )
                {
                    keyboardsToSerialize.Add( cb.Keyboard );
                }
            }
            _owner.ExportKeyboards( fileName, keyboardsToSerialize );
        }
    }
}
