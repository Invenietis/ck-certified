using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyboardEditor.KeyboardEdition
{
    public class KeyCommandTypeViewModel
    {
        /// <summary>
        /// Valid Constructor for a KeyCommandType
        /// </summary>
        /// <param name="innerName"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public KeyCommandTypeViewModel( string innerName, string name, string description, Type keyCommandParameterType )
        {
            if( !typeof( IKeyCommandParameterManager ).IsAssignableFrom( keyCommandParameterType ) ) throw new ArgumentException( String.Format( "The keyCommandParameterType ({0}) for the KeyCommandType {1} doesn't implement IKeyCommandParameter", keyCommandParameterType.ToString(), innerName ) );

            IsValid = true;
            InnerName = innerName;
            Name = name;
            Description = description;
            KeyCommandParameterType = keyCommandParameterType;
        }

        public KeyCommandTypeViewModel()
        {
        }

        /// <summary>
        /// Gets whether the current KeyCommandType InnerName is recognized by a registered command handler
        /// </summary>
        public bool IsValid { get; internal set; }

        /// <summary>
        /// Gets a user-friendly name (displayed to the user : must be multilingual)
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// A description of the command (displayed to the user : must be multilingual)
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// The name used by the command handler that is supposed ot handle the KeyCommand (ex : sendString, sendKey)
        /// This string will be set as the beggining of the KeyCommand, followed by a ":" (ex : sendstring:...)
        /// </summary>
        public string InnerName { get; internal set; }

        public Type KeyCommandParameterType { get; internal set; }
    }
}
