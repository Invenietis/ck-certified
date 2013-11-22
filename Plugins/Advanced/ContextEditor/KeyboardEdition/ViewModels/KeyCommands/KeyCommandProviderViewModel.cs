using CK.Keyboard.Model;
using CK.Plugin;
using KeyboardEditor.ViewModels;
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
        internal Dictionary<string, KeyCommandTypeViewModel> AvailableTypesInternal { get { return _availableTypes; } }

        public KeyCommandProviderViewModel()
        {
            _availableTypes = new Dictionary<string, KeyCommandTypeViewModel>();
            KeyCommand = new KeyCommandViewModel();
        }

        public KeyCommandTypeViewModel SelectedKeyCommandType
        {
            get { return KeyCommand.Type; }
            set
            {
                KeyCommand.Type = value;
                if( value != null ) KeyCommand.Parameter = KeyCommand.Type.CreateParameterManager();

                OnPropertyChanged( "KeyCommand" );
                OnPropertyChanged( "SelectedKeyCommandType" );
            }
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }

        KeyCommandViewModel _keyCommand;
        /// <summary>
        /// The KeyCommand currently displayed and used in the editor.
        /// </summary>
        public KeyCommandViewModel KeyCommand
        {
            get { return _keyCommand; }
            set
            {
                _keyCommand = value;
                OnPropertyChanged( "KeyCommand" );
            }
        }

        public void FlushCurrentKeyCommand()
        {
            SelectedKeyCommandType = null;
            KeyCommand = null;
        }

        public void InitializeKeyCommand()
        {
            KeyCommand = new KeyCommandViewModel();
        }

        public void CreateKeyCommand( string keyCommand )
        {
            KeyCommand = new KeyCommandViewModel();

            //not using the Split method in order to let a parameter use the ':' char
            int idx = keyCommand.IndexOf( ':' );
            string protocol = keyCommand.Substring( 0, idx );
            string parameter = keyCommand.Substring( idx + 1 );

            KeyCommand.Type = GetKeyCommandType( protocol );
            SelectedKeyCommandType = KeyCommand.Type;
            if( KeyCommand.Type.IsValid )
            {
                KeyCommand.Parameter = SelectedKeyCommandType.CreateParameterManager();
                if( KeyCommand.Parameter == null ) throw new ArgumentNullException( String.Format( "Null value retrieved while trying to retrieve the IKeyCommandParameterManager for the KeyCommandTypeViewModel handling the protocol '{0}'", protocol ) );

                KeyCommand.Parameter.FillFromString( parameter );
            }
            OnPropertyChanged( "KeyCommand" );
        }

        public KeyCommandTypeViewModel GetKeyCommandType( string protocol )
        {
            //If the protocol is not recognized, we'll add an Invalid KeyCommandType.
            KeyCommandTypeViewModel keyCommandType = new KeyCommandTypeViewModel( protocol, protocol );
            _availableTypes.TryGetValue( protocol, out keyCommandType );
            return keyCommandType;
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
