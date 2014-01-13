using CK.Plugin;

namespace CommonServices
{
    public interface IFileLauncherCommandHandlerService : IDynamicService
    {
        void LaunchFile(string command);
    }
}
