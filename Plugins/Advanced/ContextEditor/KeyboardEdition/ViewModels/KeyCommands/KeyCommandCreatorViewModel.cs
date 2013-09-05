using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyboardEditor.KeyboardEdition
{
    public class KeyCommandTypeProviderViewModel
    {
        Dictionary<string, KeyCommandTypeViewModel> _availableTypes;

        public IEnumerable<KeyCommandTypeViewModel> AvailableTypes { get { return _availableTypes.Values; } }

        public KeyCommandTypeProviderViewModel()
        {
            _availableTypes = new Dictionary<string, KeyCommandTypeViewModel>();

            //TODO: implement a register behavior
            _availableTypes.Add( "sendString", new KeyCommandTypeViewModel( "sendString", "Ecrire une lettre ou une phrase", "Permet d'écrire n'importe quelle chaine de caractère", typeof( SimpleKeyCommandParameterManager ) ) );
            _availableTypes.Add( "sendKey", new KeyCommandTypeViewModel( "sendKey", "Touche spéciale (F11, Entrée, Suppr ...)", "Permet de simuler la pression sur une touche sépciale comme Entrée, les touches F1..12, Effacer, Suppr etc...", typeof( SendKeyCommandParameterManager ) ) );
        }

        public KeyCommandTypeViewModel GetKeyCommandType( string innerName )
        {
            //If the innerName is not recognized, we'll add an Invalid KeyCommandType.
            KeyCommandTypeViewModel keyCommandType = new KeyCommandTypeViewModel() { InnerName = innerName, Name = innerName, IsValid = false };
            _availableTypes.TryGetValue( innerName, out keyCommandType );
            return keyCommandType;
        }
    }
}
