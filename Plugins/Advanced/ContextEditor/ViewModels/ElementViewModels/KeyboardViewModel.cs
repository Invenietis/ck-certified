using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using CK.Keyboard.Model;
using CK.Windows;

namespace ContextEditor.ViewModels
{
    public class KeyboardViewModel : PropertyChangedBase
    {
        IKeyboard _model;
        bool _isSelected;

        public KeyboardViewModel( IKeyboard model )
        {            
            _model = model;
        }

        public bool IsSelected 
        { 
            get { return _isSelected; } 
            set { _isSelected = value; NotifyOfPropertyChange( () => IsSelected ); } 
        }

        public IKeyboard Keyboard { get { return _model; } }
    }
}
