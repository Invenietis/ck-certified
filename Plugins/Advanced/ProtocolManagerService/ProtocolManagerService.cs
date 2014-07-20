using BasicCommandHandlers;
using CK.Plugin;
using CK.Plugins.SendInputDriver;
using ProtocolManagerModel;
using System;

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
