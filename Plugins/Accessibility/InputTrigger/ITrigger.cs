using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InputTrigger
{
    /// <summary>
    /// A caputred input
    /// </summary>
    public interface ITrigger
    {
        int KeyCode { get; set; }

        /// <summary>
        /// Source device
        /// </summary>
        TriggerDevice Source { get; set; }
    }

    public enum TriggerDevice
    {
        None = 0,
        Keyboard = 2,
        Civikey = 4 ,
        Pointer = 8
    }
}
