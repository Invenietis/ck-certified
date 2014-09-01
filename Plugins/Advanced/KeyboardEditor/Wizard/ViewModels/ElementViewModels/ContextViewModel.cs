using Caliburn.Micro;
using CK.Plugin.Config;

namespace KeyboardEditor.ViewModels
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
