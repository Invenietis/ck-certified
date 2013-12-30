using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ProtocolManagerModel
{
    /// <summary>
    /// This class defines a KeyCommand.
    /// It is designed to represent any kind of CiviKey action : sendstring, close civikey, minimize, toggle mode etc...
    /// </summary>
    public class VMProtocolEditor : INotifyPropertyChanged
    {
        VMProtocolEditorWrapper _wrapper;
        /// <summary>
        /// The type of action. Contains a user friendly name, its inner name (used by the command handler) and a short description of what it does.
        /// </summary>
        public VMProtocolEditorWrapper Wrapper
        {
            get { return _wrapper; }
            set
            {
                if( _wrapper != null ) _wrapper.PropertyChanged -= new PropertyChangedEventHandler( OnChildPropertyChanged );
                _wrapper = value;
                if( _wrapper != null ) _wrapper.PropertyChanged += new PropertyChangedEventHandler( OnChildPropertyChanged );

                OnPropertyChanged( "Wrapper" );
                OnPropertyChanged( "IsValid" );
            }
        }

        IProtocolParameterManager _parameterManager;
        /// <summary>
        /// The parameter of the KeyCommand. For a SendString, the parameter will be the value to be sent.
        /// This object needs to implement <see cref="IProtocolParameterManager"/>. The result of the call to <see cref="IProtocolParameterManager.GetCommandString()"/> will be set after the <see cref="Protocol"/> and its following ":".
        /// (ex: if the parameter returns "Hello" and the KeyCommand Name is "sendString", the ToString() result will be "sendString:Hello")
        /// </summary>
        public IProtocolParameterManager ParameterManager
        {
            get { return _parameterManager; }
            set
            {
                if( _parameterManager != null ) _parameterManager.PropertyChanged -= new PropertyChangedEventHandler( OnChildPropertyChanged );
                _parameterManager = value;
                if( _parameterManager != null ) _parameterManager.PropertyChanged += new PropertyChangedEventHandler( OnChildPropertyChanged );

                OnPropertyChanged( "ParameterManager" );
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
                return Wrapper != null
                    && ParameterManager != null
                    && Wrapper.IsValid
                    && ParameterManager.IsValid;
            }
        }

        public override string ToString()
        {
            if( Wrapper != null && ParameterManager != null ) return String.Format( "{0}:{1}", Wrapper.Protocol, ParameterManager.GetParameterString() );
            return String.Empty;
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
