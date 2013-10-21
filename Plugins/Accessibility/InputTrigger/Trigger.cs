using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonServices;

namespace InputTrigger
{
    public class Trigger : ITrigger
    {
        public static readonly ITrigger Default = new Trigger( 122 , TriggerDevice.Keyboard);

        public Trigger(int keyCode, TriggerDevice source) 
        {
            KeyCode = keyCode;
            Source = source;
        }

        #region ITrigger Members

        public int KeyCode { get; set; }

        public TriggerDevice Source { get; set; }

        #endregion
    }
}
