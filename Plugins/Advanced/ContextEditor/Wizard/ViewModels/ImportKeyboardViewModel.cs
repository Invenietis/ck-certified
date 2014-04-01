using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
                    UpdateCanExecuteImport();
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
            CanExecuteImport = !string.IsNullOrWhiteSpace( _filePath ) && _canExecute;
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
            _checkBoxs.Clear();

            if( !keyboardNames.Any() )
                MessageBox.Show( "Aucun clavier n'a pu être identifié dans le fichier sélectionné.", "Information", System.Windows.MessageBoxButton.OK );
            else
            {
                foreach( var k in keyboardNames )
                {
                    CheckBoxs.Add( new CheckBoxImportKeyboardViewModel( k, _keyboards.FirstOrDefault( kb => kb.Name == k ) != null ) );
                }
            }

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
                //if empty dialog box
                _owner.ImportKeyboards( _filePath, whiteList );
            }
            UpdateAlreadyExist();
            MessageBox.Show( "L'import s'est déroulé avec succès.", "Information", System.Windows.MessageBoxButton.OK );
            CleanViewModel();
            CanExecute = true; 
        }

        void CleanViewModel()
        {
            _checkBoxs.Clear();
            _filePath = string.Empty;
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
