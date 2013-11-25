using CK.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IProtocolManagerModel
{
    //TODO : Comments
    public interface IProtocolManagerService : IDynamicService
    {
        void Register( KeyCommandTypeViewModel keyCommandTypeViewModel );
        void Unregister( string protocol );

        KeyCommandProviderViewModel KeyCommandProviderViewModel { get; }
    }
}
