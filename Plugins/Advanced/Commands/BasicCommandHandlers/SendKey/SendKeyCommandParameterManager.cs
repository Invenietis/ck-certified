#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\SendKey\SendKeyCommandParameterManager.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using BasicCommandHandlers.Resources;
using CK.InputDriver;
using ProtocolManagerModel;

namespace BasicCommandHandlers
{
    public class SendKeyCommandParameterManager : IProtocolParameterManager
    {
        public SendKeyCommandParameterManager()
        {
            //TODO : use resx
            _values = new Dictionary<string, Native.KeyboardKeys>();

            _values.Add( "+ (addition)", Native.KeyboardKeys.Add );
            _values.Add( "- (soustraction)", Native.KeyboardKeys.Subtract );
            _values.Add( "* (multiplication)", Native.KeyboardKeys.Multiply );
            _values.Add( "/ (division)", Native.KeyboardKeys.Divide );

            _values.Add( "Entrée", Native.KeyboardKeys.Return );
            _values.Add( "Retour", Native.KeyboardKeys.Back );
            _values.Add( "Suppr", Native.KeyboardKeys.Delete );


            _values.Add( "Flèche vers le haut", Native.KeyboardKeys.Up );
            _values.Add( "Flèche vers le bas", Native.KeyboardKeys.Down );
            _values.Add( "Fleche vers la gauche", Native.KeyboardKeys.Left );
            _values.Add( "Flèche vers la droite", Native.KeyboardKeys.Right );

            //TODO : Where is the "beginning of the line" key ?
            _values.Add( "Echap", Native.KeyboardKeys.Escape );
            _values.Add( "Fin", Native.KeyboardKeys.End );
            _values.Add( "Insert", Native.KeyboardKeys.Insert );
            _values.Add( "Tabulation", Native.KeyboardKeys.Tab );

            _values.Add( "Page bas", Native.KeyboardKeys.PageDown );
            _values.Add( "Page haut", Native.KeyboardKeys.PageUp );

            _values.Add( "Impr écran", Native.KeyboardKeys.PrintScreen );
            _values.Add( "Verrouillage pavé numérique", Native.KeyboardKeys.NumLock );

            _values.Add( "F1", Native.KeyboardKeys.F1 );
            _values.Add( "F2", Native.KeyboardKeys.F2 );
            _values.Add( "F3", Native.KeyboardKeys.F3 );
            _values.Add( "F4", Native.KeyboardKeys.F4 );
            _values.Add( "F5", Native.KeyboardKeys.F5 );
            _values.Add( "F6", Native.KeyboardKeys.F6 );
            _values.Add( "F7", Native.KeyboardKeys.F7 );
            _values.Add( "F8", Native.KeyboardKeys.F8 );
            _values.Add( "F9", Native.KeyboardKeys.F9 );
            _values.Add( "F10", Native.KeyboardKeys.F10 );
            _values.Add( "F11", Native.KeyboardKeys.F11 );
            _values.Add( "F12", Native.KeyboardKeys.F12 );

            _values.Add( "Pause", Native.KeyboardKeys.Pause );
            _values.Add( "Lecture", Native.KeyboardKeys.Play );

            _values.Add( "Baisser son", Native.KeyboardKeys.VolumeDown );
            _values.Add( "Augmenter son", Native.KeyboardKeys.VolumeUp );
            _values.Add( "Stopper son", Native.KeyboardKeys.VolumeMute );
        }

        public IProtocolEditorRoot Root { get; set; }
        Dictionary<string, Native.KeyboardKeys> _values;
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
            Native.KeyboardKeys enumValue = Native.KeyboardKeys.A;

            if( Enum.TryParse<Native.KeyboardKeys>( parameter, out enumValue ) )
                SelectedValue = _values.Where( kvp => kvp.Value == enumValue ).Single().Key;
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }

        public bool IsValid
        {
            get
            {
                Native.KeyboardKeys enumValue = Native.KeyboardKeys.A;
                return SelectedValue != null && _values.TryGetValue( SelectedValue, out enumValue );
            }
        }

        public string GetParameterString()
        {
            if( String.IsNullOrWhiteSpace( SelectedValue ) ) return String.Empty;
            return _values[SelectedValue].ToString();
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
