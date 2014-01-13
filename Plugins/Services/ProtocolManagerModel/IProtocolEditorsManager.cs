using CK.Plugin;
using System;

namespace ProtocolManagerModel
{
    //TODO : Comments
    public interface IProtocolEditorsManager : IDynamicService
    {
        void Register( VMProtocolEditorWrapper keyCommandTypeViewModel, Type handlingService );
        void Unregister( string protocol );

        VMProtocolEditorsProvider ProtocolEditorsProviderViewModel { get; }
    }
}
