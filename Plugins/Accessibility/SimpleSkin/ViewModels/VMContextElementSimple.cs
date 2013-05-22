using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        protected virtual void OnDispose()
        {
        }

        public void Dispose()
        {
            OnDispose();
        }
    }
}
