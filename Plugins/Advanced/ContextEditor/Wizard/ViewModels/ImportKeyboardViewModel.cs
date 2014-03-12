using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CK.Keyboard.Model;
using CK.WPF.ViewModel;

namespace KeyboardEditor.Wizard.ViewModels
{
    public class ImportKeyboardViewModel
    {
        string _filePath;
        ImportKeyboard _owner;
        IKeyboardCollection _keyboards;
        List<CheckBoxImportKeyboardViewModel> _checkBoxs;

        public ImportKeyboardViewModel( ImportKeyboard owner, IKeyboardCollection keyboards )
        {
            _checkBoxs = new List<CheckBoxImportKeyboardViewModel>();
            _owner = owner;
            _keyboards = keyboards;
            OpenCommand = new VMCommand( ShowOpenFileWindow );
        }

        public string FilePath
        {
            get { return _filePath; }
            set { if( value != _filePath ) _filePath = value; }
        }

        public ICommand OpenCommand
        {
            get;
            internal set;
        }

        public List<CheckBoxImportKeyboardViewModel> CheckBoxs
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
                _filePath = dlg.FileName;
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
                _checkBoxs.Add( new CheckBoxImportKeyboardViewModel( k, _keyboards.Contains( k ) ) );
            }
        }
    }
}
