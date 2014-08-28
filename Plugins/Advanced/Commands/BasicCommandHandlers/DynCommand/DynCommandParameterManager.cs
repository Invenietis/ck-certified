#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\DynCommand\DynCommandParameterManager.cs) is part of CiviKey. 
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

using BasicCommandHandlers.Resources;
using ProtocolManagerModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BasicCommandHandlers
{
    /// <summary>
    /// IProtocolParameterManager implementation that handles the Mode actions.
    /// syntax example : mode:set,shift
    /// This example removes all other modes and sets the shift mode as the current mode.
    /// </summary>
    public class DynCommandParameterManager : IProtocolParameterManager
    {
        public IProtocolEditorRoot Root { get; set; }
        public DynCommandParameterManager()
        {
            _values = new Dictionary<string, string>();

            _values.Add( R.DynCommandHide, "togglehostminimized" );
            _values.Add( R.DynCommandClose, "shutdown" );
            _values.Add( R.DynCommandMinimize, "hideskin" );
            _values.Add( R.DynCommandWindowsKey, "windowskey" );
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
