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
