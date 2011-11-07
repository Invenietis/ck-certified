using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WPF.ViewModel;
using CK.Keyboard.Model;

namespace ViewModelsTest
{
    public class VMKeyTest : VMKey<VMContextTest, VMKeyboardTest, VMZoneTest, VMKeyTest>
    {
        public VMKeyTest( VMContextTest ctx, IKey k ) 
            : base( ctx, k )
        {
            Context.ViewModels.Add( this );
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            Context.ViewModels.Remove( this );
        }
    }
}
