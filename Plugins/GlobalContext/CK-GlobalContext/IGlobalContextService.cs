using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Specialized;
using CK.Core;
using System.Collections.ObjectModel;
using CK.Plugin;

namespace CK_GlobalContext
{
    public interface IGlobalContextService : IDynamicService
    {
        /// <summary>
        /// Gets the context.
        /// This may change at any moment.
        /// </summary>
        ObservableCollection<string> Context { get; }
    }
}
