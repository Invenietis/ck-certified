#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\ProtocolManagerModel\VMProtocolEditorWrapper.cs) is part of CiviKey. 
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
    /// Contains the description of a protocol
    /// </summary>
    public class VMProtocolEditorMetaData : INotifyPropertyChanged
    {
        /// <summary>
        /// Valid Constructor for a KeyCommandType, this object must be capable of creating on the fly its KeyCommandParameterManager.
        /// Here by using Activator.CreateInstance(Type).
        /// </summary>
        /// <param name="protocol">The protocol that is handled.</param>
        /// <param name="name">The name displayed in the list of available editors. Must be multilingual.</param>
        /// <param name="description">A description of the protocol that is handled by this object. Must be multilingual.</param>
        /// <param name="keyCommandParameterManagerType">The type of the IKeyCommandParameterMaanger that handles the parameters of this protocol. Must implement IKeyCommandParameter.</param>
        /// <param name="handlingService">The Type of the service that is going to handle the protocol. Wiil be used to add a the commandhandler to the requirement layer of the keyboard.</param>
        public VMProtocolEditorMetaData( string protocol, string name, string description, Type keyCommandParameterManagerType )
        {
            if( !typeof( IProtocolParameterManager ).IsAssignableFrom( keyCommandParameterManagerType ) ) throw new ArgumentException( String.Format( "The keyCommandParameterType ({0}) for the KeyCommandType {1} doesn't implement IKeyCommandParameter", keyCommandParameterManagerType.ToString(), protocol ) );

            Protocol = protocol;
            Name = name;
            Description = description;
            CreateParameterManager = new Func<IProtocolParameterManager>( () => { return (IProtocolParameterManager)Activator.CreateInstance( keyCommandParameterManagerType ); } );
        }

        internal Func<IProtocolParameterManager> CreateParameterManager { get; private set; }

        /// <summary>
        /// Valid Constructor for a KeyCommandType, this object must be capable of creating on the fly its KeyCommandParameterManager.
        /// Here by using the Func set as parameter.
        /// </summary>
        /// <param name="protocol">The protocol that is handled.</param>
        /// <param name="name">The name displayed in the list of available editors. Must be multilingual.</param>
        /// <param name="description">A description of the protocol that is handled by this object. Must be multilingual.</param>
        /// <param name="keyCommandParameterManagerFunc">A Func that returns an instance of an implementation of IKeyCommandParameterManager that handles a protocol. Must not return null.</param>
        /// /// <param name="handlingService">The Type of the service that is going to handle the protocol. Wiil be used to add a the commandhandler to the requirement layer of the keyboard.</param>
        public VMProtocolEditorMetaData( string protocol, string name, string description, Func<IProtocolParameterManager> keyCommandParameterManagerFunc )
        {
            Protocol = protocol;
            Name = name;
            Description = description;
            CreateParameterManager = keyCommandParameterManagerFunc;
        }

        /// <summary>
        /// Internal ctor used to create an invalid KeyCommandTypeViewModel
        /// </summary>
        internal VMProtocolEditorMetaData( string protocol, string name )
        {
            Protocol = protocol;
            Name = name;
        }

        Type _handlingService;
        /// <summary>
        /// Gets the Type of the service (ie : ISendStringService) that is supposed to handle the protocol that this Editor handles.
        /// Should be used to add Layer requirements on the edited keyboard
        /// </summary>
        public Type HandlingService { get { return _handlingService; } }

        /// <summary>
        /// Gets whether the current KeyCommandType Protocol is recognized by a registered command handler
        /// </summary>
        public bool IsValid { get { return CreateParameterManager != null; } }

        /// <summary>
        /// Gets a user-friendly name (displayed to the user : must be multilingual)
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A description of the command (displayed to the user : must be multilingual)
        /// </summary>
        public string Description { get; internal set; }

        public string Protocol { get; private set; }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
