using System;
using CK.WPF.ViewModel;
using CK.Windows;
using System.Windows.Threading;
using System.Diagnostics;

namespace SimpleSkin.ViewModels
{
    public abstract class VMContextElement : VMBase
    {
        VMContextSimpleBase _context;

        public VMContextElement( VMContextSimpleBase context )
        {
            _context = context;
        }

        /// <summary>
        /// Gets the <see cref="VMContext"/> to which this element belongs.
        /// </summary>
        public VMContextSimpleBase Context { get { return _context; } }

        internal abstract void Dispose();

        internal void SafeSet<T>( T value, Action<T> setter, bool synchronous = true )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == _context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            T val = value;
            if( synchronous )
                _context.NoFocusManager.NoFocusDispatcher.Invoke( setter, val );
            else
                _context.NoFocusManager.NoFocusDispatcher.BeginInvoke( setter, val );
        }
    }
}
