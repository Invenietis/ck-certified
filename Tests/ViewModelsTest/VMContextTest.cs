using System.Collections.Generic;
using CK.WPF.ViewModel;
using CK.Keyboard.Model;

namespace ViewModelsTest
{
    public class VMContextTest : VMContext<VMContextTest, VMKeyboardTest, VMZoneTest, VMKeyTest>
    {
        IList<VMBase> _vms;
        public IList<VMBase> ViewModels
        { 
            get 
            { 
                if(_vms == null) _vms = new List<VMBase>();
                return _vms;
            } 
        }

        public void BuggyPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );
        }

        public VMContextTest( IKeyboardContext ctx )
            : base( ctx )
        {
            ViewModels.Add( this );
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            ViewModels.Remove( this );
        }

        #region Override
        protected override VMKeyTest CreateKey( IKey k )
        {
            return new VMKeyTest( this, k );
        }

        protected override VMZoneTest CreateZone( IZone z )
        {
            return new VMZoneTest( this, z );
        }

        protected override VMKeyboardTest CreateKeyboard( IKeyboard kb )
        {
            return new VMKeyboardTest( this, kb );
        }
        #endregion
    }
}
