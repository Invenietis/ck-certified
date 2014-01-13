using System;
using CK.WPF.ViewModel;

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

        internal void ThreadSafeSet<T>( T value, Action<T> setter )
        {
            T val = value;
            Context.SkinDispatcher.Invoke( setter, val );
        }
    }
}
