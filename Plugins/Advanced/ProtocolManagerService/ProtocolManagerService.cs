#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ProtocolManagerService\ProtocolManagerService.cs) is part of CiviKey. 
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
using BasicCommandHandlers;
using CK.Plugin;
using CK.Plugins.SendInputDriver;
using ProtocolManagerModel;
using CK.Core;

namespace ProtocolManagerService
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion )]
    public class ProtocolManagerService : IPlugin, IProtocolEditorsManager
    {
        #region Plugin description

        const string PluginGuidString = "{616A53FE-3AAF-4410-8691-7CE0A97D3266}";
        const string PluginVersion = "1.0.0";
        const string PluginPublicName = "Protocol Editors Manager";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        VMProtocolEditorsProvider _vmProtocolEditorsProvider;

        public bool Setup( IPluginSetupInfo info )
        {
            _vmProtocolEditorsProvider = new VMProtocolEditorsProvider();
            return true;
        }

        public void Start()
        {
            //This should be registered through the SendStringCommandHandler (which is not in this solution)
            _vmProtocolEditorsProvider.AddEditor( "sendString", new VMProtocolEditorMetaData( "sendString", R.SendStringProtocolTitle, R.SendStringProtocolDescription, typeof( SimpleKeyCommandParameterManager ) ), typeof( ISendStringService ) );

        }

        public void Stop()
        {
        }

        public void Teardown()
        {
        }

        /// <summary>
        /// Registers the protocol into the ProtocolManager.
        /// Registered protocols are then provided to the Keyboard editor to create keys with the registered protocols
        /// </summary>
        /// <param name="VMProtocolEditorMetaData">The description of the protocol</param>
        /// <param name="handlingService">The service linked to the command handler that handles the protocol. Will be used to set the service as required if one of the keys of the keyboard uses it.</param>
        public void Register( VMProtocolEditorMetaData vmProtocolEditorWrapper, Type handlingService )
        {
            //TODO : The register should take a DataTemplate as parameter, to add it dynamically to the DataTemplateSelector used to create the keys.
            _vmProtocolEditorsProvider.AddEditor( vmProtocolEditorWrapper.Protocol, vmProtocolEditorWrapper, handlingService );
        }

        /// <summary>
        /// Registers the protocol into the ProtocolManager.
        /// Registered protocols are then provided to the Keyboard editor to create keys with the registered protocols.
        /// Use this ctor if there is no service needed to handle the protocol (see the "pause" feature)
        /// </summary>
        /// <param name="vmProtocolEditorMetaData">The description of the protocol</param>
        public void Register( VMProtocolEditorMetaData vmProtocolEditorMetaData )
        {
            Register( vmProtocolEditorMetaData, null );
        }

        public void Unregister( string protocol )
        {
            _vmProtocolEditorsProvider.RemoveEditor( protocol );
        }

        public VMProtocolEditorsProvider ProtocolEditorsProviderViewModel
        {
            get { return _vmProtocolEditorsProvider; }
        }
    }
}
