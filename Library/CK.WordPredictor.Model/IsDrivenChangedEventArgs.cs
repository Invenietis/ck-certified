using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.WordPredictor.Model
{
    public class IsDrivenChangedEventArgs : EventArgs
    {
        public bool IsDriven { get; private set; }

        public IsDrivenChangedEventArgs( bool isDriven )
        {
            IsDriven = isDriven;
        }
    }
}
