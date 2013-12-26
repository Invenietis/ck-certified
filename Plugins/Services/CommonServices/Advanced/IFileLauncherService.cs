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
        /// <summary>
        /// Give information about the file location
        /// </summary>
        FileLookup Lookup { get; }

        /// <summary>
        /// Specify the special folder (user desktop, programe files...) in which the file located.
        /// Null if the file is located in a regular path.
        /// </summary>
        Environment.SpecialFolder? FolderLocationType { get;  }

        string FileName { get; }
        /// <summary>
        /// The full file path.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Weither or not the file is found.
        /// </summary>
        bool IsLocated { get; }

        /// <summary>
        /// The icon associated with the file.
        /// </summary>
        ImageSource Icon { get; }
    }
    
    public interface IFileLocator
    {
        /// <summary>
        /// Create a new WildFile from the given path
        /// </summary>
        /// <param name="path">The target file path</param>
        /// <param name="fromRegistry">weither the file is an app stored in the registry or not</param>
        /// <returns></returns>
        IWildFile CreateWildFile(string path, bool fromRegistry);

        /// <summary>
        /// Contains all the WildFile loaded from the registry through the LoadRegistry method
        /// </summary>
        List<IWildFile> RegistryApps { get; }

        /// <summary>
        /// Try to found the path of the given WildFile.
        /// </summary>
        /// <param name="file">the file to locate. the founded path will be updated into file.Path</param>
        /// <returns>Returns if the file is found or not</returns>
        void TryLocate( IWildFile file, Action<IWildFile> callback );

        /// <summary>
        /// Generate the well formated command from the WildFile location information.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>The formated command</returns>
        string GetLocationCommand( IWildFile file );
    }

    public interface IFileLauncherService : IDynamicService
    {
        IFileLocator FileLocator { get; }

        /// <summary>
        /// Launch the file or the application
        /// </summary>
        /// <param name="file">The WildFile to launch</param>
        void Launch( IWildFile file );

        /// <summary>
        /// Get the WildFile corresponding to the given command. 
        /// The time to resolve the WildFile path may be long. That's why the result is passed through a callback function.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="loaded">A callback function that will be called when the WildFile is ready.</param>
        void LoadFromCommand( string command, Action<IWildFile> loaded );
    }
}
