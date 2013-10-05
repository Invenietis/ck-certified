using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.WindowManager.Model
{
    public enum BindingEventType
    {
        Attach,
        Detach
    }

    public class WindowBindedEventArgs : EventArgs
    {
        public BindingEventType BindingType { get; set; }

        public IBinding Binding { get; set; }
    }

    public class WindowBindingEventArgs : WindowBindedEventArgs
    {
        public bool Canceled { get; set; }
    }
}
