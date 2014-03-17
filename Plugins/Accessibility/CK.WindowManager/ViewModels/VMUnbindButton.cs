using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CK.WPF.ViewModel;

namespace CK.WindowManager
{
    public class VMUnbindButton
    {
        public VMUnbindButton(Action onClick)
        {
            UnbindCommand = new VMCommand( onClick );
        }

        public ICommand UnbindCommand
        {
            get;
            internal set;
        }
    }
}
