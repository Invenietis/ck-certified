using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WPF.ViewModel;
using CK.Keyboard.Model;

namespace ViewModelsTest
{
    public class VMZoneTest : VMZone<VMContextTest, VMKeyboardTest, VMZoneTest, VMKeyTest>
    {
        public VMZoneTest( VMContextTest ctx, IZone zone )
            : base( ctx, zone )
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
