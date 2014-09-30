#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\CommonServices\Accessibility\IFileLauncherService.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Media;
using CK.Plugin;

namespace CommonServices
{
    public enum FileLookup
    {
        Registry = 2,
        SpecialFolder = 4,
        Url = 8,
        Other = 16
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

        /// <summary>
        /// Last access time in local time
        /// </summary>
        DateTime LastAccessTime { get; }
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
