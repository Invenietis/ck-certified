#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\ProtocolManagerModel\VMProtocolEditor.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.ComponentModel;

namespace ProtocolManagerModel
{
    /// <summary>
    /// This class defines a KeyCommand.
    /// It is designed to represent any kind of CiviKey action : sendstring, close civikey, minimize, toggle mode etc...
    /// </summary>
    public class VMProtocolEditor : INotifyPropertyChanged
    {
        VMProtocolEditorMetaData _wrapper;
        /// <summary>
        /// The type of action. Contains a user friendly name, its inner name (used by the command handler) and a short description of what it does.
        /// </summary>
        public VMProtocolEditorMetaData Wrapper
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
