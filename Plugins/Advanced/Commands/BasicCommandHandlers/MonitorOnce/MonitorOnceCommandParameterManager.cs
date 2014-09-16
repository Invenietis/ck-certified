#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\MonitorOnce\MonitorOnceCommandParameterManager.cs) is part of CiviKey. 
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
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using BasicCommandHandlers.Resources;
using CK.WPF.ViewModel;
using ProtocolManagerModel;

namespace BasicCommandHandlers
{
    public class MonitorOnceCommandParameterManager : VMProtocolEditorsProvider, IProtocolParameterManager
    {
        public MonitorOnceCommandParameterManager()
        {
            _actions = new Dictionary<string, string>();
            _actions.Add( R.KeySent, "sendkey" );

            _saveCommand = new VMCommand( () =>
            {
                _innerCommand = ProtocolEditor.ToString();
                OnPropertyChanged( "IsValid" );
            } );
            _cancelCommand = new VMCommand( () => { _innerCommand = String.Empty; OnPropertyChanged( "IsValid" ); } );
        }

        IProtocolEditorRoot _root;
        public IProtocolEditorRoot Root
        {
            get
            {
                return _root;
            }
            set
            {
                _root = value;
                Initialize();
            }
        }

        private string _innerCommand;

        ICommand _saveCommand;
        public ICommand SaveCommandCommand { get { return _saveCommand; } }

        ICommand _cancelCommand;
        public ICommand CancelChangesCommand { get { return _cancelCommand; } }

        #region Action-to-listen-to selection

        Dictionary<string, string> _actions;
        public IEnumerable<string> AvailableActions { get { return _actions.Keys; } }

        string _selectedAction;
        public string SelectedAction
        {
            get { return _selectedAction; }
            set
            {
                _innerCommand = String.Empty;
                _selectedAction = value;
                OnPropertyChanged( "SelectedAction" );
                OnPropertyChanged( "IsValid" );
            }
        }

        public string ActionsTitle { get { return R.ActionToListenTo; } }
        public string NameTitle { get { return R.GiveName; } }
        public string Name { get; set; }

        private void Initialize()
        {
            EditedKeyMode = Root.EditedKeyMode;
            _availableProtocolEditors = new Dictionary<string, VMProtocolEditorWrapper>();

            foreach( var item in Root.AvailableProtocolEditors )
            {
                _availableProtocolEditors.Add( item.Protocol, item );
            }

            OnPropertyChanged( "AvailableProtocolEditors" );
        }

        public string InnerActionTitle { get { return R.MonitorOnceInnerActionTitle; } }

        public void FillFromString( string parameter )
        {
            Initialize();

            string[] splittedParameter = parameter.Split( ',' );

            SelectedAction = _actions.Where( kvp => kvp.Value == splittedParameter[0] ).Single().Key;
            Name = splittedParameter[1];
            string innerCommand = parameter.Substring( splittedParameter[0].Length + splittedParameter[1].Length + 2 );
            string innerCommandProtocol = innerCommand.Substring( innerCommand.IndexOf( ':' ) + 1 );

            CreateKeyCommand( innerCommand, Root.EditedKeyMode );

        }

        public bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace( Name )
                    && !String.IsNullOrWhiteSpace( SelectedAction )
                    && ProtocolEditor != null && ProtocolEditor.IsValid
                    && !String.IsNullOrWhiteSpace( _innerCommand );
            }
        }

        public string GetParameterString()
        {
            if( String.IsNullOrWhiteSpace( SelectedAction ) ) return String.Empty;
            return String.Format( "{0},{1},{2}", _actions[SelectedAction], Name, _innerCommand );
        }

        #endregion

        protected override void OnChildPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsValid" )
            {
                _innerCommand = ProtocolEditor != null ? ProtocolEditor.ToString() : String.Empty;
                OnPropertyChanged( "IsValid" );
            }
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }

        public new event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
