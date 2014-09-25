#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\MoveMouse\MoveMouseCommandParameterManager.cs) is part of CiviKey. 
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
using ProtocolManagerModel;

namespace BasicCommandHandlers
{
    /// <summary>
    /// Implementation of the IKeyCommandParameter interface that defines a direction (up, right, bottom, left etc..)
    /// </summary>
    public class MoveMouseCommandParameterManager : IProtocolParameterManager
    {
        public IProtocolEditorRoot Root { get; set; }
        public MoveMouseCommandParameterManager()
        {
            _values = new Dictionary<string, string>() 
            { 
                { "U", R.Up }, 
                { "R", R.Right }, 
                { "B", R.Bottom }, 
                { "L", R.Left }, 
                { "UL", R.UpLeft },
                { "UR", R.UpRight }, 
                { "BR", R.BottomRight }, 
                { "BL", R.BottomLeft  } 
            };
        }

        IDictionary<string, string> _values;
        public IEnumerable<string> AvailableValues { get { return _values.Values; } }

        string _selectedDirection;
        public string SelectedDirection
        {
            get { return _selectedDirection; }
            set
            {
                _selectedDirection = value;
                OnPropertyChanged( "SelectedDirection" );
                OnPropertyChanged( "IsValid" );
            }
        }

        int _selectedSpeed;
        public int SelectedSpeed
        {
            get { return _selectedSpeed; }
            set
            {
                _selectedSpeed = value;
                OnPropertyChanged( "SelectedSpeed" );
                OnPropertyChanged( "IsValid" );
            }
        }

        bool _snakeMode;
        public bool SnakeMode
        {
            get { return _snakeMode; }
            set
            {
                _snakeMode = value;
                OnPropertyChanged( "SnakeMode" );
            }
        }

        public void FillFromString( string parameter )
        {
            string selectedDirection = null;
            int selectedSpeed = 0;
            bool snakeMode = false;

            string[] splittedParameter = parameter.Split( ',' );
            if( splittedParameter.Length >= 2 )
            {
                if( _values.TryGetValue( splittedParameter[0].Trim(), out selectedDirection ) )
                {
                    SelectedDirection = selectedDirection;
                }
                else SelectedDirection = null;

                if( Int32.TryParse( splittedParameter[1].Trim(), out selectedSpeed ) )
                {
                    SelectedSpeed = selectedSpeed;
                }
                else SelectedSpeed = 0;

                //avoids breaking change
                if( splittedParameter.Length == 3 )
                {
                    if( bool.TryParse( splittedParameter[2].Trim(), out snakeMode ) )
                    {
                        SnakeMode = snakeMode;
                    }
                    else SnakeMode = false;
                }
            }
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }

        public bool IsValid { get { return !String.IsNullOrWhiteSpace( SelectedDirection ) && SelectedSpeed > 0; } }

        public string GetParameterString()
        {
            if( SelectedDirection == null || SelectedSpeed <= 0 ) return String.Empty;
            return String.Format( "{0}, {1}, {2}", _values.Where( x => x.Value == SelectedDirection ).Single().Key, SelectedSpeed, SnakeMode );
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
