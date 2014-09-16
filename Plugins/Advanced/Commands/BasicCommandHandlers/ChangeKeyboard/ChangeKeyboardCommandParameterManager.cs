#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\ChangeKeyboard\ChangeKeyboardCommandParameterManager.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using BasicCommandHandlers.Resources;
using CK.Keyboard.Model;
using ProtocolManagerModel;

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
