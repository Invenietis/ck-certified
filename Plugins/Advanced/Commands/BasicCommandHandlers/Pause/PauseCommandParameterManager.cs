using BasicCommandHandlers.Resources;
using ProtocolManagerModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BasicCommandHandlers
{
    /// <summary>
    /// Implementation of the IKeyCommandParameter interface that defines a integer value
    /// </summary>
    public class PauseCommandParameterManager : IProtocolParameterManager
    {
        public IProtocolEditorRoot Root { get; set; }
        public PauseCommandParameterManager()
        {
            _value = 0;
        }

        int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged( "Value" );
                OnPropertyChanged( "IsValid" );
            }
        }

        public string Title
        {
            get { return R.PauseSubTitle; }
        }

        public void FillFromString( string parameter )
        {
            int value;
            if( Int32.TryParse( parameter, out value ) )
            {
                Value = value;
            }
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }

        public bool IsValid { get { return Value > 0; } }

        public string GetParameterString()
        {
            return Value.ToString();
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
