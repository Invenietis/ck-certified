using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;

namespace KeyboardEditor.Wizard.ViewModels
{
    public class CheckBoxExportKeyboardViewModel
    {
        IKeyboard _keyboard;

        public CheckBoxExportKeyboardViewModel( IKeyboard keyboard )
        {
            _keyboard = keyboard;
        }

        public bool IsSelected { get; set; }

        public IKeyboard Keyboard 
        {
            get { return _keyboard; }
        }
    }
}
