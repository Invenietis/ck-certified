#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\ProtocolManagerModel\VMProtocolEditorsProvider.cs) is part of CiviKey. 
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
using System.ComponentModel;
using CK.Keyboard.Model;

namespace ProtocolManagerModel
{
    public interface IProtocolEditorRoot
    {
        IEnumerable<VMProtocolEditorMetaData> AvailableProtocolEditors { get; }
        IKeyMode EditedKeyMode { get; }
    }

    public class VMProtocolEditorsProvider : INotifyPropertyChanged, IProtocolEditorRoot
    {
        public IEnumerable<VMProtocolEditorMetaData> AvailableProtocolEditors { get { return _availableProtocolEditors.Values; } }

        protected Dictionary<string, VMProtocolEditorMetaData> _availableProtocolEditors { get; set; }
        protected Dictionary<string, Type> _availableProtocolHandlers { get; set; }

        public VMProtocolEditorsProvider()
        {
            _availableProtocolEditors = new Dictionary<string, VMProtocolEditorMetaData>();
            _availableProtocolHandlers = new Dictionary<string, Type>();
            ProtocolEditor = new VMProtocolEditor();
        }

        /// <summary>
        /// Used by the keyboard editor to get the service linked to a protocol. Enables adding the service into the keyboard's requirement layer.
        /// </summary>
        /// <param name="protocol">A protocol</param>
        /// <returns>The type of the service that handles the protocol set as parameter</returns>
        public Type GetHandlingService( string protocol )
        {
            Type handlingService = null;
            _availableProtocolHandlers.TryGetValue( protocol, out handlingService );
            return handlingService;
        }

        public void AddEditor( string protocol, VMProtocolEditorMetaData wrapper, Type handlingService )
        {
            _availableProtocolEditors.Add( protocol, wrapper );

            //in the case of the "pause" feature, there is no linked service
            if( handlingService != null )
                _availableProtocolHandlers.Add( protocol, handlingService );
            
            
            OnPropertyChanged( "AvailableProtocolEditors" );
        }

        public void RemoveEditor( string protocol )
        {
            if( _availableProtocolEditors.ContainsKey( protocol ) )
            {
                _availableProtocolEditors.Remove( protocol );
                _availableProtocolHandlers.Remove( protocol );
                OnPropertyChanged( "AvailableProtocolEditors" );
            }
        }



        public IKeyMode EditedKeyMode { get; set; }
        public VMProtocolEditorMetaData SelectedProtocolEditorWrapper
        {
            get { return ProtocolEditor.Wrapper; }
            set
            {
                ProtocolEditor.Wrapper = value;

                //Creating the ParameterManager on the fly
                if( value != null )
                {
                    ProtocolEditor.ParameterManager = ProtocolEditor.Wrapper.CreateParameterManager();
                    ProtocolEditor.ParameterManager.Root = this;
                }

                OnPropertyChanged( "ProtocolEditor" );
                OnPropertyChanged( "SelectedProtocolEditorWrapper" );
            }
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected VMProtocolEditor _vmProtocolEditor;
        /// <summary>
        /// The KeyCommand currently displayed and used in the editor.
        /// </summary>
        public VMProtocolEditor ProtocolEditor
        {
            get { return _vmProtocolEditor; }
            set
            {
                if( _vmProtocolEditor != null ) _vmProtocolEditor.PropertyChanged += new PropertyChangedEventHandler( OnChildPropertyChanged );
                _vmProtocolEditor = value;
                if( _vmProtocolEditor != null ) _vmProtocolEditor.PropertyChanged += new PropertyChangedEventHandler( OnChildPropertyChanged );

                OnPropertyChanged( "ProtocolEditor" );
            }
        }

        protected virtual void OnChildPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
        }

        public void FlushCurrentProtocolEditor()
        {
            SelectedProtocolEditorWrapper = null;
            InitializeProtocolEditor( null );
        }

        public void InitializeProtocolEditor( IKeyMode editedKeyMode )
        {
            EditedKeyMode = editedKeyMode;
            ProtocolEditor = new VMProtocolEditor();
        }

        public void CreateKeyCommand( string keyCommand, IKeyMode editedKeyMode )
        {
            EditedKeyMode = editedKeyMode;
            ProtocolEditor = new VMProtocolEditor();

            //not using the Split method in order to let a parameter use the ':' char
            int idx = keyCommand.IndexOf( ':' );
            string protocol = keyCommand.Substring( 0, idx );
            string parameter = keyCommand.Substring( idx + 1 );

            ProtocolEditor.Wrapper = GetProtocolEditorWrapper( protocol );
            SelectedProtocolEditorWrapper = ProtocolEditor.Wrapper;
            if( ProtocolEditor.Wrapper.IsValid )
            {
                ProtocolEditor.ParameterManager = SelectedProtocolEditorWrapper.CreateParameterManager();
                if( ProtocolEditor.ParameterManager == null ) throw new ArgumentNullException( String.Format( "Null value retrieved while trying to retrieve the IKeyCommandParameterManager for the KeyCommandTypeViewModel handling the protocol '{0}'", protocol ) );

                ProtocolEditor.ParameterManager.Root = this;
                ProtocolEditor.ParameterManager.FillFromString( parameter );
            }
            OnPropertyChanged( "ProtocolEditor" );
        }

        /// <summary>
        /// Gets the <see cref="VMProtocolEditorMetaData"/> for the specified protocol.
        /// If the protocol is not handled, returns an empty <see cref="VMProtocolEditorMetaData"/>, with a IsValid property returning false
        /// </summary>
        /// <param name="protocol">The protocol (ex : sendString, sendKey, keyboardswitch...)</param>
        /// <returns>The <see cref="VMProtocolEditorMetaData"/> corresponding to the protocol set as parameter. if the returned object's IsValid property returns false, the protocol is not handled</returns>
        public VMProtocolEditorMetaData GetProtocolEditorWrapper( string protocol )
        {
            //If the protocol is not recognized, we'll add an Invalid KeyCommandType.
            VMProtocolEditorMetaData editorWrapper = new VMProtocolEditorMetaData( protocol, protocol );
            if( _availableProtocolEditors.TryGetValue( protocol, out editorWrapper ) )
            {
                return editorWrapper;
            }
            return new VMProtocolEditorMetaData( protocol, protocol );
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
