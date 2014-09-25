#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ViewModels\ImportKeyboardViewModel.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CK.Keyboard.Model;
using CK.WPF.ViewModel;

namespace KeyboardEditor.Wizard.ViewModels
{
    public class ImportKeyboardViewModel : VMBase
    {
        string _filePath;
        ImportKeyboard _owner;
        IKeyboardCollection _keyboards;
        ObservableCollection<CheckBoxImportKeyboardViewModel> _checkBoxs;
        bool _canExecute;
        bool _canExecuteImport;

        public ImportKeyboardViewModel( ImportKeyboard owner, IKeyboardCollection keyboards )
        {
            _checkBoxs = new ObservableCollection<CheckBoxImportKeyboardViewModel>();
            _owner = owner;
            _keyboards = keyboards;
            _canExecute = true;
            _canExecuteImport = false;
            OpenCommand = new VMCommand( ShowOpenFileWindow, o => CanExecute );
            ImportCommand = new VMCommand( Importkeyboards, o => CanExecuteImport );
        }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if( value != _filePath )
                {
                    _filePath = value;
                    OnPropertyChanged();
                }
            }
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
                    UpdateCanExecuteImport();
                }
            }
        }

        public bool CanExecuteImport
        {
            get { return _canExecuteImport; }
            set
            {
                if( value != _canExecuteImport )
                {
                    _canExecuteImport = value;
                    OnPropertyChanged();
                }
            }
        }
        void UpdateCanExecuteImport()
        {
            CanExecuteImport = !string.IsNullOrWhiteSpace( _filePath ) && _canExecute && _checkBoxs.Any( cb => cb.IsSelected );
        }

        public ICommand OpenCommand
        {
            get;
            internal set;
        }

        public ICommand ImportCommand
        {
            get;
            internal set;
        }

        public ObservableCollection<CheckBoxImportKeyboardViewModel> CheckBoxs
        {
            get { return _checkBoxs; }
        }

        void ShowOpenFileWindow()
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".xml";
            dlg.Filter = "CiviKey keyboard (.xml)|*.xml";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if( result == true )
            {
                CleanViewModel();
                // Open document
                FilePath = dlg.FileName;

                if( !string.IsNullOrWhiteSpace( _filePath ) )
                {
                    CreateCheckBox( _owner.GetImportKeyboardNames( _filePath ) );
                }
            }
        }

        bool CheckAlreadyExistKeyboards()
        {
            foreach( var cb in _checkBoxs )
            {
                if( cb.IsSelected && cb.AlreadyExist )
                {
                    return MessageBox.Show( "Un ou plusieurs des claviers que vous voulez importer existent déjà dans votre civiKey. \n Ces derniers seront écrasés par les claviers que vous venez de sélectionner.\n Etes-vous sur de vouloir continuer ?", "Confirmer l'import", System.Windows.MessageBoxButton.YesNo ) == MessageBoxResult.Yes;
                }
            }
            return true;
        }

        void CreateCheckBox( IEnumerable<string> keyboardNames )
        {
            CheckBoxs.Clear();

            if( !keyboardNames.Any() )
                MessageBox.Show( "Aucun clavier n'a pu être identifié dans le fichier sélectionné.", "Information", System.Windows.MessageBoxButton.OK );
            else
            {
                CheckBoxImportKeyboardViewModel checkbox = null;
                foreach( var k in keyboardNames )
                {
                    checkbox = new CheckBoxImportKeyboardViewModel( k, _keyboards.FirstOrDefault( kb => kb.Name == k ) != null );
                    CheckBoxs.Add( checkbox );
                    checkbox.PropertyChanged += checkbox_PropertyChanged;
                }
            }

        }

        void checkbox_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsSelected" ) UpdateCanExecuteImport();
        }

        void UpdateAlreadyExist()
        {
            foreach( var cb in _checkBoxs )
            {
                cb.AlreadyExist = _keyboards.FirstOrDefault( kb => kb.Name == cb.KeyboardName ) != null;
            }
        }

        void Importkeyboards()
        {
            CanExecute = false;
            if( CheckAlreadyExistKeyboards() )
            {
                string whiteList = GenerateWhiteList();
                try
                {
                    _owner.ImportKeyboards( _filePath, whiteList );
                }
                catch
                {
                    MessageBox.Show( "Un des claviers sélectionnés est corrompu.", "Information", System.Windows.MessageBoxButton.OK );
                    CanExecute = true;
                    return;
                }
                UpdateAlreadyExist();
                MessageBox.Show( "L'opération d'import s'est déroulée avec succès.", "Information", System.Windows.MessageBoxButton.OK );
            }
            CleanViewModel();
            CanExecute = true; 
        }

        void CleanViewModel()
        {
            foreach( var cb in _checkBoxs ) cb.PropertyChanged -= checkbox_PropertyChanged;
            CheckBoxs.Clear();
            FilePath = string.Empty;
            CanExecuteImport = false;
        }

        string GenerateWhiteList()
        {
            string whiteList = string.Empty;
            foreach( var cb in _checkBoxs )
            {
                if( cb.IsSelected )
                {
                    if( string.IsNullOrEmpty( whiteList ) ) whiteList += cb.KeyboardName;
                    else whiteList += "|" + cb.KeyboardName;
                }
            }
            return whiteList;
        }
    }
}
