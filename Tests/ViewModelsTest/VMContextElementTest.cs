using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WPF.ViewModel;

namespace ViewModelsTest
{
    internal class VMContextElementTest : VMContextElement<VMContextTest, VMKeyboardTest, VMZoneTest, VMKeyTest>
    {
        public VMContextElementTest( VMContextTest context )
            : base( context )
        {
        }
    }
}
