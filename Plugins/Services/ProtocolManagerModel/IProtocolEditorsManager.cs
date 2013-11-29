using CK.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
