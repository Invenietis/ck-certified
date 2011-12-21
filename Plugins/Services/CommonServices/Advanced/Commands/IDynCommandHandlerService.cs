
using CK.Plugin;
namespace CommonServices
{
    public interface IDynCommandHandlerService : IDynamicService
    {
        void Exec( string actionKey );
    }
}