using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CK.WPF.ViewModel;

namespace SimpleSkin.ViewModels
{
    public abstract class VMContextElement : VMBase
    {
        VMContextSimple _context;

        public VMContextElement( VMContextSimple context )
        {
            _context = context;
        }

        /// <summary>
        /// Gets the <see cref="VMContext"/> to which this element belongs.
        /// </summary>
        public VMContextSimple Context { get { return _context; } }

        internal abstract void Dispose();

        internal void ThreadSafeSet<T>( T value, Action<T> setter )
        {
            T val = value;
            Context.SkinDispatcher.Invoke( setter, val );
        }
    }
}
