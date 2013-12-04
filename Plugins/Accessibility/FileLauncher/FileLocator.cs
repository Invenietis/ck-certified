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
        /// Contains all the WildApp loaded from the registry through the LoadRegistry method
        /// </summary>
        public static List<WildApp> RegistryApps { get; private set; }

        public static Dictionary<string, Environment.SpecialFolder> SpecialFolders { get; private set; }

        /// <summary>
        /// Load all the WildApp from the registry into RegistryApps
        /// </summary>
        public static void LoadRegistry()
        {
            RegistryApps = new List<WildApp>();
            //From App Paths
            {
                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths");
                foreach (string s in key.GetSubKeyNames())
                {
                    string path = key.OpenSubKey(s).GetValue("", null) as string;
                    if (path != null && File.Exists(path) && !RegistryApps.Exists(x => x.Path == path))
                    {
                        RegistryApps.Add(new WildApp(Path.GetFileName(path)){ Path=path });
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
                                RegistryApps.Add(new WildApp(Path.GetFileName(f)) { Path = f });
                            }
                        }
                    }
                }
            }

            RegistryApps.Sort((a, b) => {
                return a.FileName.CompareTo(b.FileName);
            });
        }

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

        public static WildFile GetFileFromPath(string path)
        {
            WildFile file = new WildFile(Path.GetFileName(path)) { Path = path };
            file.FolderLocationType = GetSpecialFolder(path);

            if (file.FolderLocationType != null)
            {
                file.Lookup = FileLookup.SpecialFolder;
                string fullpath = Environment.GetFolderPath(file.FolderLocationType.Value);
                file.Path = path.Substring(fullpath.Length);
            }
            else
            {
                file.Lookup = FileLookup.Other;
            }

            return file;
        }

        /// <summary>
        /// Found the path of the given WildFile and set it to it
        /// </summary>
        /// <param name="file">the file to locate. the founded path will be updated</param>
        /// <returns>Return the found path</returns>
        public static string Locate(WildFile file)
        {
            switch(file.Lookup)
            {
                case FileLookup.Registry: return LocateFromRegistry(file);
                case FileLookup.SpecialFolder: return LocateFromSpecialDirectory(file);
                case FileLookup.Other: return LocateFromFileSystem(file);
                default: return LocateFromFileSystem(file);
            }
        }

        public static string GetLocationCommand(WildFile file)
        {
            string cmd = file.FileName + "," + (int) file.Lookup;
            if (file.Lookup == FileLookup.SpecialFolder)
            {
                cmd += "," + file.Path + "," + (int)file.FolderLocationType;
            }
            if (file.Lookup == FileLookup.Other)
            {
                cmd += "," + file.Path;
            }

            return cmd;
        }

        static string LocateFromRegistry(WildFile file)
        {
            WildFile found = RegistryApps.FirstOrDefault(f => f.FileName == file.FileName);
            if (found != null) return file.Path = found.Path;

            return null;
        }

        static string LocateFromSpecialDirectory(WildFile file)
        {
            file.Path = Environment.GetFolderPath(file.FolderLocationType.Value) + file.Path;
            return file.Path;
        }

        static string LocateFromFileSystem(WildFile file)
        {
            throw new NotImplementedException();
        }

        static Environment.SpecialFolder? GetSpecialFolder(string path)
        {
            foreach(string spacialPath in SpecialFolders.Keys)
            {
                if (path.StartsWith(spacialPath, StringComparison.InvariantCultureIgnoreCase)) 
                    return SpecialFolders[spacialPath];
            }

            return null;
        }
    }
}
