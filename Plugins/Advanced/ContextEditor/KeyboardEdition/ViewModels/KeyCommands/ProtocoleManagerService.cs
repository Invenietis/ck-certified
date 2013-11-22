using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyboardEditor.KeyboardEdition
{
   [Plugin( ProtocoleManagerService.PluginIdString,
        PublicName = ProtocoleManagerService.PluginPublicName,
        Version = ProtocoleManagerService.PluginIdVersion )]
    public class ProtocoleManagerService : IPlugin, IProtocolManagerService
    {
        const string PluginIdString = "{616A53FE-3AAF-4410-8691-7CE0A97D3266}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Protocol Manager Service";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        KeyCommandProviderViewModel _keyCommandProviderViewModel;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            _keyCommandProviderViewModel = new KeyCommandProviderViewModel();
            return true;
        }

        public void Start()
        {
            //TODO : These should be added through the Register method. -> the IKeycommandParameterManager implementations should be created next to the corresponding CommandManager.
            //TODO : The register should take a DataTemplate as parameter, to add it dynamically to the DataTemplateSelector used to create the keys.

            _keyCommandProviderViewModel.AvailableTypesInternal.Add( "sendString", new KeyCommandTypeViewModel( "sendString", "Ecrire une lettre ou une phrase", "Permet d'écrire n'importe quelle chaine de caractère", typeof( SimpleKeyCommandParameterManager ) ) );
            _keyCommandProviderViewModel.AvailableTypesInternal.Add( "sendKey", new KeyCommandTypeViewModel( "sendKey", "Touche spéciale (F11, Entrée, Suppr ...)", "Permet de simuler la pression sur une touche spéciale comme Entrée, les touches F1..12, Effacer, Suppr etc...", typeof( SendKeyCommandParameterManager ) ) );
            _keyCommandProviderViewModel.AvailableTypesInternal.Add( "keyboardswitch", new KeyCommandTypeViewModel( "keyboardswitch", "Changer de clavier", "Permet de changer de clavier", new Func<SwitchKeyboardCommandParameterManager>( () => { return new SwitchKeyboardCommandParameterManager( KeyboardContext.Service.Keyboards.ToList() ); } ) ) );
        }

        public void Stop()
        {
            _keyCommandProviderViewModel.AvailableTypesInternal.Clear();
        }

        public void Teardown()
        {
        }

        public void Register( KeyCommandTypeViewModel keyCommandTypeViewModel )
        {
            _keyCommandProviderViewModel.AvailableTypesInternal.Add( keyCommandTypeViewModel.Protocol, keyCommandTypeViewModel );
        }

        public void Unregister( string protocol )
        {
            if( _keyCommandProviderViewModel.AvailableTypesInternal.ContainsKey( protocol ) )
            {
                _keyCommandProviderViewModel.AvailableTypesInternal.Remove( protocol );
            }
        }

        public KeyCommandProviderViewModel KeyCommandProviderViewModel
        {
            get { return _keyCommandProviderViewModel; }
        }
    }
}
