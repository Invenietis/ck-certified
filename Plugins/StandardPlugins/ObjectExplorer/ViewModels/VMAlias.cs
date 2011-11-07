using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Core;
using System.Windows.Input;
using CK.WPF.ViewModel;
using System.Windows;
using System.Windows.Forms;
using CK.WPF;

namespace CK.StandardPlugins.ObjectExplorer
{
    public class VMAlias<T> : VMICoreElement
        where T : VMICoreElement
    {
        T _wrapped;

        public override object Data { get { return _wrapped; } }

        public VMAlias( T wrapped, VMIBase parent ) :
            base( wrapped.VMIContext, parent )
        {
            _wrapped = wrapped;
        }
    }
}
