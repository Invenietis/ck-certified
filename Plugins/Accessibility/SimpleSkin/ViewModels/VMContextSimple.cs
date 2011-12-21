using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using CK.Core;
using CK.Context;

namespace SimpleSkin.ViewModels
{
    internal class VMContextSimple : VMContext<VMContextSimple,VMKeyboardSimple,VMZoneSimple,VMKeySimple>
    {
        IPluginConfigAccessor _config;
        public IPluginConfigAccessor Config
        {
            get
            {
                if( _config == null ) _config = Context.GetService<IPluginConfigAccessor>( true );
                return _config;
            }
        }

        public VMContextSimple( IContext ctx, IKeyboardContext kbctx )
            : base( ctx, kbctx.Keyboards.Context )
        {
        }

        protected override VMKeySimple CreateKey( IKey k )
        {
            return new VMKeySimple( this, k );
        }

        protected override VMZoneSimple CreateZone( IZone z )
        {
            return new VMZoneSimple( this, z );
        }

        protected override VMKeyboardSimple CreateKeyboard( IKeyboard kb )
        {
            return new VMKeyboardSimple( this, kb );
        }
    }
}
