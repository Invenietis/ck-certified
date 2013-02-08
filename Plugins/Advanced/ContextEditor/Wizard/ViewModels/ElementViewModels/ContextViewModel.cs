using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using CK.Windows;

namespace ContextEditor.ViewModels
{
    public class ContextViewModel : PropertyChangedBase
    {
        IUriHistory _model;
        bool _isSelected;
        ContextListViewModel _parent;

        public ContextViewModel( ContextListViewModel parent, IUriHistory model )
        {
            _parent = parent;
            _model = model;
        }

        public bool IsSelected 
        { 
            get { return _isSelected; } 
            set { _isSelected = value; NotifyOfPropertyChange( () => IsSelected ); } 
        }

        public IUriHistory Context { get { return _model; } }
    }
}
