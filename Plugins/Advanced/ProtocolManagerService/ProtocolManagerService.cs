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
* Copyright © 2007-2012, 
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

namespace ProtocolManagerService
{
    [Plugin( ProtocolManagerService.PluginIdString,
         PublicName = ProtocolManagerService.PluginPublicName,
         Version = ProtocolManagerService.PluginIdVersion )]
    public class ProtocolManagerService : IPlugin, IProtocolEditorsManager
    {
        const string PluginIdString = "{616A53FE-3AAF-4410-8691-7CE0A97D3266}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Protocol Editors Manager";

        VMProtocolEditorsProvider _vmProtocolEditorsProvider;

        public bool Setup( IPluginSetupInfo info )
        {
            _vmProtocolEditorsProvider = new VMProtocolEditorsProvider();
            return true;
        }

        public void Start()
        {
            //This should be registered through the SendStringCommandHandler (which is not in this solution)
            _vmProtocolEditorsProvider.AddEditor( "sendString", new VMProtocolEditorWrapper( "sendString", R.SendStringProtocolTitle, R.SendStringProtocolDescription, typeof( SimpleKeyCommandParameterManager ) ), typeof( ISendStringService ) );
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
        }

        public void Register( VMProtocolEditorWrapper vmProtocolEditorWrapper, Type handlingService )
        {
            //TODO : The register should take a DataTemplate as parameter, to add it dynamically to the DataTemplateSelector used to create the keys.
            _vmProtocolEditorsProvider.AddEditor( vmProtocolEditorWrapper.Protocol, vmProtocolEditorWrapper, handlingService );
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
