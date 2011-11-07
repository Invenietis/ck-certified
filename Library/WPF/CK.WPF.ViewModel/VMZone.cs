using System.Collections.ObjectModel;
using CK.Keyboard.Model;

namespace CK.WPF.ViewModel
{
    public abstract class VMZone<TC, TB, TZ, TK> : VMContextElement<TC, TB, TZ, TK>
        where TC : VMContext<TC, TB, TZ, TK>
        where TB : VMKeyboard<TC, TB, TZ, TK>
        where TZ : VMZone<TC, TB, TZ, TK>
        where TK : VMKey<TC, TB, TZ, TK>
    {
        IZone _zone;
        ObservableCollection<TK> _keys;

        public ObservableCollection<TK> Keys { get { return _keys; } }

        public VMZone( TC context, IZone zone )
            : base( context )
        {
            _zone = zone;
            _keys = new ObservableCollection<TK>();

            foreach( IKey key in _zone.Keys )
            {
                TK k = Context.Obtain( key );
                Keys.Add( k );
            }
        }

        protected override void OnDispose()
        {
            foreach( TK key in Keys )
            {
                key.Dispose();
            }
        }
    }
}
