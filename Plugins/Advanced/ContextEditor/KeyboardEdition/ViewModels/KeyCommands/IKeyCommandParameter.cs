using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace KeyboardEditor.KeyboardEdition
{
    public interface IKeyCommandParameterManager : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets a string containing the Parameter's GetCommandString return value and populates the object with it.
        /// </summary>
        /// <param name="parameter">The parameter as a string (is the result of the GetCommandString method of this object's implementation.</param>
        void FillFromString( string parameter );

        /// <summary>
        /// Returns the representation of this object's implementation's value.
        /// This value is to be processed by a corresponding CommandHandler.
        /// </summary>
        /// <returns>Returns the representation of this object's implementation's value. This value is to be processed by a corresponding CommandHandler.</returns>
        string GetParameterString();

        /// <summary>
        /// Gets whether the parameter is valid (ie: can be safely saved as is)
        /// </summary>
        bool IsValid { get; }
    }
}
