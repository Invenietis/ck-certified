#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\SendString\SimpleKeyCommandParameter.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using ProtocolManagerModel;
using System;

namespace BasicCommandHandlers
{
    /// <summary>
    /// Most simple implementation of the IKeyCommandParameter interface.
    /// The parameter is set as string value.
    /// Getting the Command string returns the value.
    /// This implementation is all the sendString command needs.
    /// </summary>
    public class SimpleKeyCommandParameterManager : IProtocolParameterManager
    {
        string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged( "Value" );
                OnPropertyChanged( "IsValid" );
            }
        }

        public void FillFromString( string parameter )
        {
            Value = parameter;
        }

        public string GetParameterString()
        {
            return Value;
        }

        public bool IsValid
        {
            get { return !String.IsNullOrEmpty( Value ); }
        }

        public void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;


        public IProtocolEditorRoot Root { get; set; }
    }
}
