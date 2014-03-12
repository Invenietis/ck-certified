using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

        public ImportKeyboardViewModel( ImportKeyboard owner, IKeyboardCollection keyboards )
        {
            _checkBoxs = new ObservableCollection<CheckBoxImportKeyboardViewModel>();
            _owner = owner;
            _keyboards = keyboards;
            OpenCommand = new VMCommand( ShowOpenFileWindow );
            ImportCommand = new VMCommand( Importkeyboards );
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
                CheckAlreadyExistKeyboards();
            }
        }

        void CheckAlreadyExistKeyboards()
        {
            if( !string.IsNullOrWhiteSpace( _filePath ) )
            {
                CreateCheckBox( _owner.GetImportKeyboardNames( _filePath ) );
            }
        }

        void CreateCheckBox( IEnumerable<string> keyboardNames )
        {
            foreach( var k in keyboardNames )
            {
                CheckBoxs.Add( new CheckBoxImportKeyboardViewModel( k, _keyboards.FirstOrDefault( kb => kb.Name == k ) != null ) );
            }
        }

        void Importkeyboards()
        {
            string whiteList = GenerateWhiteList();
            //if empty dialog box
            _owner.ImportKeyboards( _filePath, GenerateWhiteList() );
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
