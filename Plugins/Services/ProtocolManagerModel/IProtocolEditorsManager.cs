using CK.Plugin;
using System;

namespace ProtocolManagerModel
{
    //TODO : Comments
    public interface IProtocolEditorsManager : IDynamicService
    {
        void Register( VMProtocolEditorMetaData keyCommandTypeViewModel, Type handlingService );
        void Register( VMProtocolEditorMetaData keyCommandTypeViewModel );

        void Unregister( string protocol );

        VMProtocolEditorsProvider ProtocolEditorsProviderViewModel { get; }
    }
}
