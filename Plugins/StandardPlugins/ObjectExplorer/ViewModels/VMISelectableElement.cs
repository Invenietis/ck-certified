using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CK.WPF.ViewModel;
using System.Windows;

namespace CK.StandardPlugins.ObjectExplorer
{
    public class VMISelectableElement : VMIBase, ISelectableElement
    {
        public bool IsSelected
        {
            get { return VMIContext.SelectedElement == this; }
            set { if( value ) VMIContext.SelectedElement = this; }
        }

        public void SelectedChanged()
        {
            OnPropertyChanged("IsSelected");
        }

        ICommand _gotoCommand;
        public ICommand GoTo
        {
            get
            {
                if (_gotoCommand == null)
                {
                    _gotoCommand = new VMCommand<VMISelectableElement>(
                    (e) =>
                    {
                        if( e is VMAlias<VMIService> ) VMIContext.SelectedElement = (VMIService)((VMAlias<VMIService>)e).Data;
                        else if( e is VMAlias<VMIPlugin> ) VMIContext.SelectedElement = (VMIPlugin)((VMAlias<VMIPlugin>)e).Data;
                        else if( e is VMIService ) VMIContext.SelectedElement = (VMIService)e;
                        else VMIContext.SelectedElement = e;
                    });
                }
                return _gotoCommand;
            }
        }

        public VMISelectableElement( VMIContextViewModel ctx, VMIBase parent )
            : base( ctx, parent )
        {
        }
    }
}
