using BasicCommandHandlers;
using CK.Plugin;
using ProtocolManagerModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            _vmProtocolEditorsProvider.AvailableProtocolEditors.Add( "sendString", new VMProtocolEditorWrapper( "sendString", R.SendStringProtocolTitle, R.SendStringProtocolDescription, typeof( SimpleKeyCommandParameterManager ) ) );
        }

        public void Stop()
        {
            _vmProtocolEditorsProvider.AvailableProtocolEditors.Clear();
        }

        public void Teardown()
        {
        }

        public void Register( VMProtocolEditorWrapper vmProtocolEditorWrapper )
        {
            //TODO : The register should take a DataTemplate as parameter, to add it dynamically to the DataTemplateSelector used to create the keys.
            _vmProtocolEditorsProvider.AvailableProtocolEditors.Add( vmProtocolEditorWrapper.Protocol, vmProtocolEditorWrapper );
        }

        public void Unregister( string protocol )
        {
            if( _vmProtocolEditorsProvider.AvailableProtocolEditors.ContainsKey( protocol ) )
            {
                _vmProtocolEditorsProvider.AvailableProtocolEditors.Remove( protocol );
            }
        }

        public VMProtocolEditorsProvider KeyCommandProviderViewModel
        {
            get { return _vmProtocolEditorsProvider; }
        }
    }
}
