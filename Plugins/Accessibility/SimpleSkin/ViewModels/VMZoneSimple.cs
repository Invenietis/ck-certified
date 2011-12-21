using CK.WPF.ViewModel;
using CK.Keyboard.Model;

namespace SimpleSkin.ViewModels
{
    internal class VMZoneSimple : VMZone<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple>
    {
        public VMZoneSimple( VMContextSimple ctx, IZone zone ) 
            : base( ctx, zone )
        {
        }
    }
}
