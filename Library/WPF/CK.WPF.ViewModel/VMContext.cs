using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using CK.Keyboard.Model;
using CK.Context;

namespace CK.WPF.ViewModel
{
    public abstract class VMContext<TC, TB, TZ, TK> : VMBase, IDisposable
        where TC : VMContext<TC,TB,TZ,TK>
        where TB : VMKeyboard<TC, TB, TZ, TK>
        where TZ : VMZone<TC, TB, TZ, TK>
        where TK : VMKey<TC, TB, TZ, TK>
    {
        IKeyboardContext _kbctx;
        IContext _ctx;
        Dictionary<object, VMContextElement<TC, TB, TZ, TK>> _dic;
        TB _currentKeyboard;
        EventHandler<KeyboardEventArgs> _evKeyboardCreated;
        EventHandler<CurrentKeyboardChangedEventArgs> _evCurrentKeyboardChanged;
        EventHandler<KeyboardEventArgs> _evKeyboardDestroyed;
        ObservableCollection<TB> _keyboards;

        //public event EventHandler KeyboardChanging;
        //public event EventHandler KeyboardChanged;

        public IKeyboardContext KeyboardContext { get { return _kbctx; } }

        public IContext Context { get { return _ctx; } }

        public ObservableCollection<TB> Keyboards
        {
            get { return _keyboards; }
        }

        public TB Obtain( IKeyboard keyboard )
        {
            TB k = FindViewModel<TB>( keyboard );
            if( k == null ) throw new Exception( "Context mismatch." );
            return k;
        }

        public TZ Obtain( IZone zone )
        {
            TZ z = FindViewModel<TZ>( zone );
            if( z == null )
            {
                if( zone.Context != _kbctx )
                    throw new Exception( "Context mismatch." );
                z = CreateZone( zone );
                _dic.Add( zone, z );
            }
            return z;
        }
        
        public TK Obtain( IKey key )
        {
            TK k = FindViewModel<TK>( key );
            if( k == null )
            {
                if( key.Context != _kbctx )
                    throw new Exception( "Context mismatch." );
                k = CreateKey( key );
                _dic.Add( key, k );
            }
            return k;
        }

        public TB Keyboard { get { return _currentKeyboard; } }

        public VMContext( IContext ctx, IKeyboardContext kbctx )
        {
            _dic = new Dictionary<object, VMContextElement<TC, TB, TZ, TK>>();
            _keyboards = new ObservableCollection<TB>();

            _kbctx = kbctx;
            _ctx = ctx;
            if( _kbctx.Keyboards.Count() > 0 )
            {
                foreach( IKeyboard keyboard in _kbctx.Keyboards )
                {
                    TB kb = CreateKeyboard( keyboard );
                    _dic.Add( keyboard, kb );
                    _keyboards.Add( kb );
                }
                _currentKeyboard = Obtain( _kbctx.CurrentKeyboard );
            }
            
            _evKeyboardCreated = new EventHandler<KeyboardEventArgs>( OnKeyboardCreated );
            _evCurrentKeyboardChanged = new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
            _evKeyboardDestroyed = new EventHandler<KeyboardEventArgs>( OnKeyboardDestroyed );
            
            _kbctx.Keyboards.KeyboardCreated += _evKeyboardCreated;
            _kbctx.CurrentKeyboardChanged += _evCurrentKeyboardChanged;
            _kbctx.Keyboards.KeyboardDestroyed += _evKeyboardDestroyed;
        }

        protected abstract TB CreateKeyboard( IKeyboard kb );
        protected abstract TZ CreateZone( IZone z );
        protected abstract TK CreateKey( IKey k );

        T FindViewModel<T>( object m )
            where T : VMContextElement<TC, TB, TZ, TK>
        {
            VMContextElement<TC, TB, TZ, TK> vm;
            _dic.TryGetValue( m, out vm );
            return (T)vm;
        }

        public void Dispose()
        {
            OnDispose();
            _kbctx.Keyboards.KeyboardCreated -= _evKeyboardCreated;
            _kbctx.CurrentKeyboardChanged -= _evCurrentKeyboardChanged;
            _kbctx.Keyboards.KeyboardDestroyed -= _evKeyboardDestroyed;
            foreach( VMContextElement<TC, TB, TZ, TK> vm in _dic.Values ) vm.Dispose();
            _dic.Clear();
        }

        protected virtual void OnDispose()
        {
        }

        internal void OnModelDestroy( object m )
        {
            VMContextElement<TC, TB, TZ, TK> vm;
            if( _dic.TryGetValue( m, out vm ) )
            {
                vm.Dispose();
                _dic.Remove( m );
            }
        }

        #region OnXXXXXXXXX
        void OnKeyboardCreated( object sender, KeyboardEventArgs e )
        {
            TB k = CreateKeyboard( e.Keyboard );
            _dic.Add( e.Keyboard, k );
            _keyboards.Add( k );
        }

        //Post 4.0 version (there has been changes in the WPF notify engine, we now need to close the whole window and re-create it
        //void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        //{
        //    if ( e.Current != null )
        //    {
        //        if ( KeyboardChanging != null ) KeyboardChanging( this, new EventArgs() );
        //        _currentKeyboard = Obtain( e.Current );
        //        if ( KeyboardChanged != null ) KeyboardChanged( this, new EventArgs() );
        //    }
        //}

        //Pre 4.0 version (there has been changes in the WPF notify engine, when processing the X binding,
        //the engine moves the window, which SETS the Y, before processing it
        void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            if ( e.Current != null )
            {
                _currentKeyboard = Obtain( e.Current );
                OnPropertyChanged( "Keyboard" );
            }
        }

        void OnKeyboardDestroyed( object sender, KeyboardEventArgs e )
        {
            _keyboards.Remove( Obtain( e.Keyboard ) );
            OnModelDestroy( e.Keyboard );
        }
        #endregion
    }
}
