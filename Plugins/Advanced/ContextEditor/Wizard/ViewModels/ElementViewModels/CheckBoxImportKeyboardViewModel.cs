using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyboardEditor.Wizard.ViewModels
{
    public class CheckBoxImportKeyboardViewModel
    {
        string _keyboardName;

        public CheckBoxImportKeyboardViewModel( string keyboardName, bool alreadyExist )
        {
            _keyboardName = keyboardName;
            AlreadyExist = alreadyExist;
        }

        public bool AlreadyExist { get; private set; }

        public bool IsSelected { get; set; }

        public string KeyboardName
        {
            get { return _keyboardName; }
        }
    }
}
