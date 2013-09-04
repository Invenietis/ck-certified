using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyboardEditor.KeyboardEdition
{
    /// <summary>
    /// Most simple implementation of the IKeyCommandParameter interface.
    /// The parameter is set as string value.
    /// Getting the Command string returns the value.
    /// This implementation is all the sendString command needs.
    /// </summary>
    public class SimpleKeyCommandParameterManager : IKeyCommandParameterManager
    {
        public string Value { get; set; }
        public void FillFromString( string parameter )
        {
            Value = parameter;
        }

        public string GetParameterString()
        {
            return Value;
        }
    }
}
