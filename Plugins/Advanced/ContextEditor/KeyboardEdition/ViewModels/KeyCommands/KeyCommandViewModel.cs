using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace KeyboardEditor.KeyboardEdition
{
    /// <summary>
    /// This class defines a KeyCommand.
    /// It is designed to represent any kind of CiviKey action : sendstring, close civikey, minimize, toggle mode etc...
    /// </summary>
    public class KeyCommandViewModel : INotifyPropertyChanged
    {
        KeyCommandTypeViewModel _type;
        /// <summary>
        /// The type of action. Contains a user friendly name, its inner name (used by the command handler) and a small description of what it does.
        /// </summary>
        public KeyCommandTypeViewModel Type
        {
            get { return _type; }
            set
            {
                if( _type != null ) _type.PropertyChanged -= new PropertyChangedEventHandler( OnChildPropertyChanged );
                _type = value;
                if( _type != null ) _type.PropertyChanged += new PropertyChangedEventHandler( OnChildPropertyChanged );

                OnPropertyChanged( "Type" );
                OnPropertyChanged( "IsValid" );
            }
        }

        IKeyCommandParameterManager _parameter;
        /// <summary>
        /// The parameter of the KeyCommand. For a SendString, the parameter will be the value to be sent.
        /// This object needs to implement <see cref="IKeyCommandParameterManager"/>. The result of the call to <see cref="IKeyCommandParameterManager.GetCommandString()"/> will be set after the <see cref="Protocol"/> and its following ":".
        /// (ex: if the parameter returns "Hello" and the KeyCommand Name is "sendString", the ToString() result will be "sendString:Hello")
        /// </summary>
        public IKeyCommandParameterManager Parameter
        {
            get { return _parameter; }
            set
            {
                if( _parameter != null ) _parameter.PropertyChanged -= new PropertyChangedEventHandler( OnChildPropertyChanged );
                _parameter = value;
                if( _parameter != null ) _parameter.PropertyChanged += new PropertyChangedEventHandler( OnChildPropertyChanged );

                OnPropertyChanged( "Parameter" );
                OnPropertyChanged( "IsValid" );
            }
        }

        private void OnChildPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsValid" ) OnPropertyChanged( "IsValid" );
        }

        public bool IsValid
        {
            get
            {
                return Type != null
                    && Parameter != null
                    && Type.IsValid
                    && Parameter.IsValid;
            }
        }

        public override string ToString()
        {
            return String.Format( "{0}:{1}", Type.Protocol, Parameter.GetParameterString() );
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
