using BasicCommandHandlers;
using CK.Plugin;
using IProtocolManagerModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolManagerService
{
   [Plugin( ProtocolManagerService.PluginIdString,
        PublicName = ProtocolManagerService.PluginPublicName,
        Version = ProtocolManagerService.PluginIdVersion )]
    public class ProtocolManagerService : IPlugin, IProtocolManagerService
    {
        const string PluginIdString = "{616A53FE-3AAF-4410-8691-7CE0A97D3266}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Protocol Manager Service";

        KeyCommandProviderViewModel _keyCommandProviderViewModel;

        public bool Setup( IPluginSetupInfo info )
        {
            _keyCommandProviderViewModel = new KeyCommandProviderViewModel();
            return true;
        }

        public void Start()
        {
            //This should be registered through the SendStringCommandHandler (which is not in this solution)
            _keyCommandProviderViewModel.AvailableTypes.Add( "sendString", new KeyCommandTypeViewModel( "sendString", "Ecrire une lettre ou une phrase", "Permet d'écrire n'importe quelle chaine de caractère", typeof( SimpleKeyCommandParameterManager ) ) );
        }

        public void Stop()
        {
            _keyCommandProviderViewModel.AvailableTypes.Clear();
        }

        public void Teardown()
        {
        }

        public void Register( KeyCommandTypeViewModel keyCommandTypeViewModel )
        {
            //TODO : The register should take a DataTemplate as parameter, to add it dynamically to the DataTemplateSelector used to create the keys.
            _keyCommandProviderViewModel.AvailableTypes.Add( keyCommandTypeViewModel.Protocol, keyCommandTypeViewModel );
        }

        public void Unregister( string protocol )
        {
            if( _keyCommandProviderViewModel.AvailableTypes.ContainsKey( protocol ) )
            {
                _keyCommandProviderViewModel.AvailableTypes.Remove( protocol );
            }
        }

        public KeyCommandProviderViewModel KeyCommandProviderViewModel
        {
            get { return _keyCommandProviderViewModel; }
        }
    }
}
