using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using CK.Plugin;

namespace CommonServices
{
    public enum FileLookup
    {
        Registry = 2,
        SpecialFolder = 4,
        Other = 8
    }

    public interface IWildFile : IComparable
    {
        FileLookup Lookup { get; }
        Environment.SpecialFolder? FolderLocationType { get;  }
        string FileName { get; }
        string Path { get; }
        bool IsLocated { get; }
        ImageSource Icon { get; }
    }
    
    public interface IFileLocator
    {
        IWildFile CreateWildFile(string path, bool fromRegistry);

        IWildFile CreateWildFile( string filename );

        List<IWildFile> RegistryApps { get; }

        void TryLocate( IWildFile file, Action<IWildFile> callback );

        string GetLocationCommand( IWildFile file );
    }

    public interface IFileLauncherService : IDynamicService
    {
        IFileLocator FileLocator { get; }

        void Launch( IWildFile file );

        void LoadFromCommand( string command, Action<IWildFile> loaded );
    }
}
