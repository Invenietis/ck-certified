#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\KeySequence\KeySequenceCommandParameterManager.cs) is part of CiviKey. 
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
using CK.InputDriver.Replay;
using CK.WPF.ViewModel;
using ProtocolManagerModel;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace BasicCommandHandlers
{
    public class KeySequenceCommandParameterManager : IProtocolParameterManager
    {
        KeyboardReplayer _replayer;
        StringBuilder _sb;

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

        string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged( "Name" );
                OnPropertyChanged( "IsValid" );
            }
        }

        public KeySequenceCommandParameterManager()
        {
            _sb = new StringBuilder();
            _replayer = new KeyboardReplayer( k =>
            {
                _sb.Append( k.ToString() );
                _sb.Append( "+" );
            } );
        }

        bool _isRecording;
        public bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                _isRecording = value;
                OnPropertyChanged( "IsRecording" );
                OnPropertyChanged( "IsValid" );
            }
        }

        public string NameTitle { get { return R.KeySequenceNameTitle; } }
        public string StartRecordingTitle { get { return R.StartRecordingTitle; } }
        public string StopRecordingTitle { get { return R.StopRecordingTitle; } }
        public string IsRecordingText { get { return R.IsRecordingText; } }

        ICommand _startRecordingCommand;
        public ICommand StartRecordingCommand
        {
            get
            {
                if( _startRecordingCommand == null )
                {
                    _startRecordingCommand = new VMCommand( () =>
                    {
                        Debug.Assert( !IsRecording );
                        IsRecording = true;

                        _sb.Clear();
                        _replayer.Start();
                    } );
                }
                return _startRecordingCommand;
            }
        }

        ICommand _stopRecordingCommand;
        public ICommand StopRecordingCommand
        {
            get
            {
                if( _stopRecordingCommand == null )
                {
                    _stopRecordingCommand = new VMCommand( () =>
                    {
                        Debug.Assert( IsRecording );

                        _replayer.Stop();
                        _sb.Length = _sb.Length - 1;//removing the last "+"
                        Value = _sb.ToString();

                        IsRecording = false;
                    } );
                }
                return _stopRecordingCommand;
            }
        }

        public void FillFromString( string parameter )
        {
            Name = parameter.Substring( 0, parameter.IndexOf( ',' ) );
            Value = parameter.Substring( parameter.IndexOf( ',' ) + 1 );
        }

        public string GetParameterString()
        {
            return String.Format( "{0},{1}", Name, Value );
        }

        public bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace( Value )
                    && !String.IsNullOrWhiteSpace( Name );
            }
        }

        public void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;


        public IProtocolEditorRoot Root { get; set; }
    }
}
