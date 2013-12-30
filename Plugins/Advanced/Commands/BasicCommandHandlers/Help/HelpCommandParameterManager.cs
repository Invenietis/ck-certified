using BasicCommandHandlers.Resources;
using CK.Plugins.SendInputDriver;
using ProtocolManagerModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicCommandHandlers
{
    public class HelpCommandParameterManager : IProtocolParameterManager
    {
        public IProtocolEditorRoot Root { get; set; }
        public HelpCommandParameterManager()
        {
            _values = new Dictionary<string, string>();
            _values.Add( R.ShowHelpAction, "show" );
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
