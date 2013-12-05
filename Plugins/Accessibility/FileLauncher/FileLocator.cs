using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileLauncher
{
    public static class FileLocator
    {
        /// <summary>
        /// Contains all the WildFile loaded from the registry through the LoadRegistry method
        /// </summary>
        public static List<WildFile> RegistryApps { get; private set; }

        public static Dictionary<string, Environment.SpecialFolder> SpecialFolders { get; private set; }

        /// <summary>
        /// Load all the WildFile from the registry into RegistryApps
        /// </summary>
        public static void LoadRegistry()
        {
            RegistryApps = new List<WildFile>();
            //From App Paths
            {
                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths");
                foreach (string s in key.GetSubKeyNames())
                {
                    string path = key.OpenSubKey(s).GetValue("", null) as string;
                    if (path != null && File.Exists(path) && !RegistryApps.Exists(x => x.Path == path))
                    {
                        RegistryApps.Add(GetFileFromPath(path));
                    }
                }
            }
            //Uninstall
            {
                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                foreach (string s in  key.GetSubKeyNames())
                {
                    string path = key.OpenSubKey(s).GetValue("InstallLocation", null) as string;
                    if (path != null && Directory.Exists(path))
                    {
                        foreach (string f in Directory.GetFiles(path, "*.exe", SearchOption.TopDirectoryOnly))
                        {
                            if (File.Exists(f) && !f.Contains("unins") && !RegistryApps.Exists(x => x.Path == f))
                            {
                                RegistryApps.Add(GetFileFromPath(f));
                            }
                        }
                    }
                }
            }

            RegistryApps.Sort((a, b) => {
                return a.FileName.CompareTo(b.FileName);
            });
        }

        /// <summary>
        /// Load the special folders path
        /// </summary>
        public static void LoadSpecialFolders()
        {
            SpecialFolders = new Dictionary<string, Environment.SpecialFolder>();
            foreach( var specialFolder in Enum.GetValues(typeof (Environment.SpecialFolder)))
            {
                var sp = (Environment.SpecialFolder) specialFolder;
                string fp = Environment.GetFolderPath(sp);
                if(fp.Length == 0) continue;

                SpecialFolders[fp] = sp;
            }
        }

        /// <summary>
        /// Load resources
        /// </summary>
        public static void Init()
        {
            LoadSpecialFolders();
            LoadRegistry();
        }

        /// <summary>
        /// Creates a WildFile from the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static WildFile GetFileFromPath(string path, bool fromRegistry = false)
        {
            WildFile file = new WildFile(Path.GetFileName(path)) { Path = path };
            file.FolderLocationType = GetSpecialFolder(path);

            file.Lookup = fromRegistry ? FileLookup.Registry : FileLookup.SpecialFolder;

            if (file.FolderLocationType != null)
            {
                
            }
            else if (!fromRegistry)
            {
                file.Lookup = FileLookup.Other;
            }

            return file;
        }

        /// <summary>
        /// Try to found the path of the given WildFile.
        /// </summary>
        /// <param name="file">the file to locate. the founded path will be updated into file.Path</param>
        /// <returns>Returns if the file is found or not</returns>
        public static bool TryLocate( WildFile file)
        {
            switch(file.Lookup)
            {
                case FileLookup.Registry: LocateFromRegistry(file);
                    break;
                case FileLookup.SpecialFolder: LocateFromSpecialDirectory(file);
                    break;
                case FileLookup.Other: LocateFromFileSystem(file);
                    break;
                default: LocateFromFileSystem(file);
                    break;
            }
            //Last chance
            if (!file.IsLocated)
            {
                string path = TryLocatePath(file.Path);
                if (path != null) file.Path = path;
            }

            return file.IsLocated;
        }

        /// <summary>
        /// Generate the well formated command from the WildFile location information.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>The formated command</returns>
        public static string GetLocationCommand(WildFile file)
        {
            string cmd = String.Format("{0},{1}", file.FileName, (int) file.Lookup);
            if (file.FolderLocationType != null) //false if  the path does not contains a special location : file.Lookup = SpecialFolder | Registry
            {
                string sPath = Environment.GetFolderPath(file.FolderLocationType.Value);
                string path = file.Path.Substring(sPath.Length); //Get the path without the special location

                cmd += String.Format(",{0},{1}", path, (int)file.FolderLocationType);
            }
            else //file.Lookup = Other
            {
                cmd += String.Format(",{0}", file.Path);
            }

            return cmd;
        }

        static void LocateFromRegistry(WildFile file)
        {
            WildFile found = RegistryApps.FirstOrDefault(f => f.FileName == file.FileName);
            file.Path = found.Path;
        }

        static void LocateFromSpecialDirectory(WildFile file)
        {
            file.Path = Environment.GetFolderPath(file.FolderLocationType.Value) + file.Path;
    
        }

        static void LocateFromFileSystem(WildFile file)
        {

        }

        /// <summary>
        /// Get the SpecialFolder from the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>the matched SpecialFolder or null</returns>
        static Environment.SpecialFolder? GetSpecialFolder(string path)
        {
            foreach(string spacialPath in SpecialFolders.Keys)
            {
                if (path.StartsWith(spacialPath, StringComparison.InvariantCultureIgnoreCase)) 
                    return SpecialFolders[spacialPath];
            }

            return null;
        }

        /// <summary>
        /// Try to locate a not found file path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Null if not found</returns>
        static string TryLocatePath(string path)
        {
            if (path.Length == 0) return null;
            if(File.Exists(path)) return path;

            var d = Path.GetDirectoryName(path);
            if (Directory.Exists(d))
            {
                var tokens = path.Split('\\');

                //Child inspection :  find a folder that match with a path token
                foreach(string childDirectory in Directory.GetDirectories(d))
                {
                    string filePath = childDirectory + @"\" + Path.GetFileName(path);
                    if (File.Exists(filePath)) return filePath;

                    for (int i = 1; i < tokens.Length - 1; i++) //Ignore the disc and the filename
                    {
                        var child = childDirectory + @"\" + tokens[i];
                        
                        if (Directory.Exists(child)) return TryLocatePath(child + @"\" + Path.GetFileName(path));
                    }
                }
            }
            if (Directory.GetParent(d) == null) return null;

            return TryLocatePath( Directory.GetParent(d).FullName + @"\" + Path.GetFileName(path));
        }
    }
}
