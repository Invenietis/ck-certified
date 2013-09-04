using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyboardEditor.KeyboardEdition
{
    /// <summary>
    /// This class defines a KeyCommand.
    /// It is designed to represent any kind of CiviKey action : sendstring, close civikey, minimize, toggle mode etc...
    /// </summary>
    public class KeyCommandViewModel
    {
        /// <summary>
        /// The type of action. Contains a user friendly name, its inner name (user by the commadn handler) and a small description of what it does.
        /// </summary>
        public KeyCommandTypeViewModel Type { get; set; }

        /// <summary>
        /// The parameter of the KeyCommand. For a SendString, the parameter will be the value to be sent.
        /// This object needs to implement <see cref="IKeyCommandParameterManager"/>. The result of the call to <see cref="IKeyCommandParameterManager.GetCommandString()"/> will be set after the <see cref="InnerName"/> and its following ":".
        /// (ex: if the parameter returns "Hello" and the KeyCommand Name is "sendString", the result will be "sendString:Hello")
        /// </summary>
        public IKeyCommandParameterManager Parameter { get; set; }

        public override string ToString()
        {
            return String.Format( "{0}:{1}", Type.InnerName, Parameter.GetParameterString() );
        }

        public IEnumerable<KeyCommandTypeViewModel> AvailableTypes { get { return _typeProvider.AvailableTypes; } }
        KeyCommandTypeProviderViewModel _typeProvider;

        /// <summary>
        /// Constructor of a KeyCommandViewModel. Takes the return value of its ToString override to populate itself.
        /// </summary>
        /// <param name="commandString">The command saved by CiviKey in the <see cref="KeyMode">KeyMode's</see> KeyCommand tag. Must have the "actionType:parameters" format</param>
        public KeyCommandViewModel( KeyCommandTypeProviderViewModel typeProvider, string keyCommand )
        {
            if( !keyCommand.Contains( ':' ) ) throw new ArgumentException( "A KeyCommand set as parameter to create a KeyCommandViewModel must have the \"actionType:parameters\" format" );

            _typeProvider = typeProvider;

            //not using the Split method in order to let a parameter use the ':' char
            string innerName = keyCommand.Substring( 0, keyCommand.IndexOf( ':' ) );
            string parameter = keyCommand.Substring( keyCommand.IndexOf( ':' ) + 1 );

            Type = _typeProvider.GetKeyCommandType( innerName );
            if( Type.IsValid )
                Parameter = CreateKeyCommandParameter( Type, parameter );
        }

        private IKeyCommandParameterManager CreateKeyCommandParameter( KeyCommandTypeViewModel keyCommandType, string parameter )
        {
            IKeyCommandParameterManager keyCommandParameterManager = (IKeyCommandParameterManager)Activator.CreateInstance( keyCommandType.KeyCommandParameterType );
            keyCommandParameterManager.FillFromString( parameter );

            return keyCommandParameterManager;
        }
    }
}
