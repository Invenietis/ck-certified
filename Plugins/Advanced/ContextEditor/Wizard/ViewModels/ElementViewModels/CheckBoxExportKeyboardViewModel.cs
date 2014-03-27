using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CK.Keyboard.Model;
using CK.WPF.ViewModel;

namespace KeyboardEditor.Wizard.ViewModels
{
    public class CheckBoxExportKeyboardViewModel : VMBase
    {
        IKeyboard _keyboard;
        bool _isSelected;

        public CheckBoxExportKeyboardViewModel( IKeyboard keyboard )
        {
            _keyboard = keyboard;
            _isSelected = false;
            CheckCommand = new VMCommand( () => IsSelected = (IsSelected)?false:true );
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

        public IKeyboard Keyboard 
        {
            get { return _keyboard; }
        }

        public ICommand CheckCommand
        {
            get;
            private set;
        }
    }
}
