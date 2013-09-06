using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace KeyboardEditor.KeyboardEdition
{
    public class KeyCommandProviderViewModel : INotifyPropertyChanged
    {
        Dictionary<string, KeyCommandTypeViewModel> _availableTypes;
        public IEnumerable<KeyCommandTypeViewModel> AvailableTypes { get { return _availableTypes.Values; } }

        public KeyCommandTypeViewModel SelectedKeyCommandType
        {
            get { return KeyCommand.Type; }
            set
            {
                KeyCommand.Type = value;
                if( value != null ) KeyCommand.Parameter = CreateKeyCommandParameter( KeyCommand.Type );
                OnPropertyChanged( "KeyCommand" );
                OnPropertyChanged( "SelectedKeyCommandType" );
            }
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }

        KeyCommandViewModel _keyCommand;
        public KeyCommandViewModel KeyCommand
        {
            get { return _keyCommand; }
            set
            {
                _keyCommand = value;
                OnPropertyChanged( "KeyCommand" );
            }
        }

        public KeyCommandProviderViewModel()
        {
            _availableTypes = new Dictionary<string, KeyCommandTypeViewModel>();

            //TODO: implement a register behavior
            _availableTypes.Add( "sendString", new KeyCommandTypeViewModel( "sendString", "Ecrire une lettre ou une phrase", "Permet d'écrire n'importe quelle chaine de caractère", typeof( SimpleKeyCommandParameterManager ) ) );
            _availableTypes.Add( "sendKey", new KeyCommandTypeViewModel( "sendKey", "Touche spéciale (F11, Entrée, Suppr ...)", "Permet de simuler la pression sur une touche sépciale comme Entrée, les touches F1..12, Effacer, Suppr etc...", typeof( SendKeyCommandParameterManager ) ) );

            KeyCommand = new KeyCommandViewModel( this );
        }

        public void FlushCurrentKeyCommand()
        {
            SelectedKeyCommandType = null;
            KeyCommand = null;
        }

        public void InitializeKeyCommand()
        {
            KeyCommand = new KeyCommandViewModel( this );
        }

        public void CreateKeyCommand( string keyCommand )
        {
            KeyCommand = new KeyCommandViewModel( this );

            //not using the Split method in order to let a parameter use the ':' char
            string innerName = keyCommand.Substring( 0, keyCommand.IndexOf( ':' ) );
            string parameter = keyCommand.Substring( keyCommand.IndexOf( ':' ) + 1 );

            KeyCommand.Type = GetKeyCommandType( innerName );
            SelectedKeyCommandType = KeyCommand.Type;
            if( KeyCommand.Type.IsValid )
                KeyCommand.Parameter = CreateKeyCommandParameter( KeyCommand.Type, parameter );
            OnPropertyChanged( "KeyCommand" );
        }

        public KeyCommandTypeViewModel GetKeyCommandType( string innerName )
        {
            //If the innerName is not recognized, we'll add an Invalid KeyCommandType.
            KeyCommandTypeViewModel keyCommandType = new KeyCommandTypeViewModel() { InnerName = innerName, Name = innerName };
            _availableTypes.TryGetValue( innerName, out keyCommandType );
            return keyCommandType;
        }

        private IKeyCommandParameterManager CreateKeyCommandParameter( KeyCommandTypeViewModel keyCommandType )
        {
            IKeyCommandParameterManager keyCommandParameterManager = (IKeyCommandParameterManager)Activator.CreateInstance( keyCommandType.KeyCommandParameterType );
            return keyCommandParameterManager;
        }

        private IKeyCommandParameterManager CreateKeyCommandParameter( KeyCommandTypeViewModel keyCommandType, string parameter )
        {
            IKeyCommandParameterManager keyCommandParameterManager = (IKeyCommandParameterManager)Activator.CreateInstance( keyCommandType.KeyCommandParameterType );
            keyCommandParameterManager.FillFromString( parameter );

            return keyCommandParameterManager;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
