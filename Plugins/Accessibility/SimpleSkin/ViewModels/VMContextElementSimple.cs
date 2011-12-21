using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WPF.ViewModel;

namespace SimpleSkin.ViewModels
{
    internal class VMContextElementSimple : VMContextElement<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple>
    {
        public VMContextElementSimple( VMContextSimple context )
            : base( context )
        {
        }
    }
}
