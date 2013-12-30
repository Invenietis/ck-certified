using BasicCommandHandlers.Resources;
using CK.Keyboard.Model;
using CK.Plugins.SendInputDriver;
using ProtocolManagerModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicCommandHandlers
{
    /// <summary>
    /// IProtocolParameterManager implementation that handles the Mode actions.
    /// syntax example : mode:set,shift
    /// This example removes all other modes and sets the shift mode as the current mode.
    /// </summary>
    public class DynCommandParameterManager : IProtocolParameterManager
    {
        public DynCommandParameterManager()
        {
            _values = new Dictionary<string, string>();

            _values.Add( "Hide the keyboard", "hideskin" );
            _values.Add( "Close CiviKey", "shutdown" );
            _values.Add( "minimize the keyboard", "togglehostminimized" );
            _values.Add( "push the 'Windows' key", "windowskey" );
        }

        Dictionary<string, string> _values;
        public IEnumerable<string> AvailableValues { get { return _values.Keys; } }

        string _selectedValue;
        public string SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                _selectedValue = value;
                OnPropertyChanged( "SelectedValue" );
                OnPropertyChanged( "IsValid" );
            }
        }

        public string Title { get { return R.SendKeyProtocolSubtitle; } }

        public void FillFromString( string parameter )
        {
            SelectedValue = _values.Where( kvp => kvp.Value == parameter ).Single().Key;
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }

        public bool IsValid
        {
            get { return !String.IsNullOrWhiteSpace( SelectedValue ); }
        }

        public string GetParameterString()
        {
            if( String.IsNullOrWhiteSpace( SelectedValue ) ) return String.Empty;
            return _values[SelectedValue];
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
