﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyboardEditor.KeyboardEdition
{
    /// <summary>
    /// Most simple implementation of the IKeyCommandParameter interface.
    /// The parameter is set as string value.
    /// Getting the Command string returns the value.
    /// This implementation is all the sendString command needs.
    /// </summary>
    public class SimpleKeyCommandParameterManager : IKeyCommandParameterManager
    {
        string _value;
        public string Value { get { return _value; } set { _value = value; } }
        public void FillFromString( string parameter )
        {
            Value = parameter;
        }

        public string GetParameterString()
        {
            return Value;
        }

        public void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
