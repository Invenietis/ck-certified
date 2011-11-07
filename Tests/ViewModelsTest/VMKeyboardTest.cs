using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WPF.ViewModel;
using CK.Keyboard.Model;

namespace ViewModelsTest
{
    public class VMKeyboardTest : VMKeyboard<VMContextTest, VMKeyboardTest, VMZoneTest, VMKeyTest>
    {
        public VMKeyboardTest( VMContextTest ctx, IKeyboard kb )
            : base( ctx, kb )
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
