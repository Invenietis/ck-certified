using System;

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
