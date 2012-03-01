using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WPF.ViewModel;
using System.Windows.Input;

namespace CK.Plugins.ObjectExplorer.ViewModels.LogViewModels
{
    public class VMLogOutputCategory : VMBase
    {
        public VMLogOutputCategory( VMLogOutputContainer holder, string name )
        {
            _holder = holder;
            _name = name;
            _isVisible = true;
        }

        VMCommand _toggleFilterCommand;
        VMLogOutputContainer _holder;
        bool _isVisible;
        string _name;

        public string Name { get { return _name; } }
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                OnPropertyChanged( "IsVisible" );
                _holder.PropagateVisibilityChanged( this );
            }
        }
        public ICommand ToggleFilterCommand
        {
            get
            {
                if( _toggleFilterCommand == null )
                {
                    _toggleFilterCommand = new VMCommand( () =>
                    {
                        if( _isVisible )
                            IsVisible = false;
                        else
                            IsVisible = true;
                    } );
                }
                return _toggleFilterCommand;
            }
        }
    }

}
