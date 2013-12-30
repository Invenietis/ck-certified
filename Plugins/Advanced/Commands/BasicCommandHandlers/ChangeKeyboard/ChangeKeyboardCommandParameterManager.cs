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
    /// Implementation of the IKeyCommandParameter interface that enables switching to a keyboard.
    /// The parameter is set as string value.
    /// Getting the Command string returns the value.
    /// This implementation is the name of the keyboard.
    /// </summary>
    public class ChangeKeyboardCommandParameterManager : IProtocolParameterManager
    {
        public IProtocolEditorRoot Root { get; set; }
        public ChangeKeyboardCommandParameterManager( ICollection<IKeyboard> keyboards )
        {
            _values = new Dictionary<string, IKeyboard>();
            foreach( var keyboard in keyboards )
            {
                _values.Add( keyboard.Name, keyboard );
            }
        }

        Dictionary<string, IKeyboard> _values;
        public IEnumerable<string> AvailableValues { get { return _values.Keys; } }

        string _selectedKeyboard;
        public string SelectedValue
        {
            get { return _selectedKeyboard; }
            set
            {
                _selectedKeyboard = value;
                OnPropertyChanged( "SelectedValue" );
                OnPropertyChanged( "IsValid" );
            }
        }

        public string Title { get { return R.KeyboardProtocolSubtitle; } }

        public void FillFromString( string parameter )
        {
            IKeyboard selectedKeyboard = null;
            if( _values.TryGetValue( parameter, out selectedKeyboard ) )
            {
                SelectedValue = selectedKeyboard.Name;
            }
            else SelectedValue = null;
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }

        public bool IsValid { get { return SelectedValue != null; } }

        public string GetParameterString()
        {
            if( SelectedValue == null ) return String.Empty;
            return SelectedValue;
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
