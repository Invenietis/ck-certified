namespace CK.WPF.ViewModel
{
    /// <summary>
    /// </summary>
    public abstract class VMContextElement<TC, TB, TZ, TK> : VMBase
        where TC : VMContext<TC, TB, TZ, TK>
        where TB : VMKeyboard<TC, TB, TZ, TK>
        where TZ : VMZone<TC, TB, TZ, TK>
        where TK : VMKey<TC, TB, TZ, TK>
    {
        TC _context;

        protected VMContextElement( TC context )
        {
            _context = context;
        }

        /// <summary>
        /// Gets the <see cref="VMContext"/> to wich this element belongs.
        /// </summary>
        public TC Context { get { return _context; } }

        /// <summary>
        /// Internal method called by this <see cref="Context"/> only.
        /// </summary>
        internal void Dispose()
        {
            OnDispose();
        }

        protected virtual void OnDispose()
        {
        }

    }
}