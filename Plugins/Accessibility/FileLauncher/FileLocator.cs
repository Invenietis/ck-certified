using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using CommonServices;

namespace FileLauncher
{
    public class FileLocator : IFileLocator
    {
        //An iterator
        static int it;
        static readonly int INTERATION_LIMIT = 500;
        static Stopwatch _watch;

        /// <summary>
        /// Contains all the WildFile loaded from the registry through the LoadRegistry method
        /// </summary>
        public List<IWildFile> RegistryApps { get; private set; }

        public static Dictionary<string, Environment.SpecialFolder> SpecialFolders { get; private set; }

        public FileLocator()
        {
            Init();
        }

        /// <summary>
        /// Load all the WildFile from the registry into RegistryApps
        /// </summary>
        public void LoadRegistry()
        {
            RegistryApps = new List<IWildFile>();
            //From App Paths
            {
                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths");
                foreach (string s in key.GetSubKeyNames())
                {
                    string path = key.OpenSubKey(s).GetValue("", null) as string;
                    if (path != null && File.Exists(path) && !RegistryApps.Exists(x => x.Path == path))
                    {
                        RegistryApps.Add(new WildFile(path, true));
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
                                RegistryApps.Add(new WildFile( f, true ) );
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
        public void Init()
        {
            LoadSpecialFolders();
            LoadRegistry();
        }

        /// <summary>
        /// Try to found the path of the given WildFile.
        /// </summary>
        /// <param name="file">the file to locate. the founded path will be updated into file.Path</param>
        /// <returns>Returns if the file is found or not</returns>
        public void TryLocate( IWildFile file, Action<IWildFile> callback )
        {
            WildFile wFile = file as WildFile;
            switch(file.Lookup)
            {
                case FileLookup.Registry:
                    LocateFromRegistry( wFile );
                    if( !wFile.IsLocated ) //True if found in the registry
                        LocateFromSpecialDirectory( wFile );
                    break;
                case FileLookup.SpecialFolder:
                    LocateFromSpecialDirectory( wFile );
                    break;
                case FileLookup.Other:
                    LocateFromSpecialDirectory( wFile );
                    break;
            }

            //Last chance
            if( !wFile.IsLocated )
            {
                Task.Factory.StartNew( () => {
                    string path = TryLocatePath( wFile.Path );
                    
                    //
                    if( path != null ) wFile.Path = path;

                    callback( wFile );
                } );
            }
            else
            {
                callback( wFile );
            }
        }

        /// <summary>
        /// Generate the well formated command from the WildFile location information.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>The formated command</returns>
        public string GetLocationCommand( IWildFile file )
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

        void LocateFromRegistry(WildFile file)
        {
            IWildFile found = RegistryApps.FirstOrDefault(f => f.FileName == file.FileName);
            file.Path = found.Path;
        }

        void LocateFromSpecialDirectory(WildFile file)
        {
            if (file.FolderLocationType == null) return;

            file.Path = Environment.GetFolderPath(file.FolderLocationType.Value) + file.Path;
        }

        /// <summary>
        /// Get the SpecialFolder from the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>the matched SpecialFolder or null</returns>
        public static Environment.SpecialFolder? GetSpecialFolder(string path)
        {
            foreach(string specialPath in SpecialFolders.Keys)
            {
                if (path.StartsWith(specialPath, StringComparison.InvariantCultureIgnoreCase)) 
                    return SpecialFolders[specialPath];
            }

            return null;
        }

        /// <summary>
        /// Try to locate a not found file path : 
        /// First, we look into the current directory and test if the file exist in a direct sub directory.
        /// Then if the file can't be found, we search a folder in sub directories listed in folders.
        /// If there is no match, we get the parent folder. if there is a match, we get in the sub directory.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="folders">The folder names that may contains the file</param>
        /// <param name="exclude">Path to ignore</param>
        /// <returns></returns>
        string TryLocatePath( string path, string[] folders = null, List<string> exclude = null )
        {
            if( exclude == null )
            {
                exclude = new List<string>();
                it = 0;
            }
            if( folders == null ) folders = Path.GetDirectoryName(path).Split( new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries );
            ++it;

            if( path.Length == 0 || it > INTERATION_LIMIT )
            {
                if( it > INTERATION_LIMIT ) Console.WriteLine("Iteration limit reached !");
                return null;
            }
            if(File.Exists(path)) return path;

            var currDirectory = Path.GetDirectoryName(path);
            if (Directory.Exists(currDirectory) && !exclude.Contains( currDirectory ))
            {
               exclude.Add( currDirectory );
   
                //Child inspection :  find a folder that match with a path token
                foreach(string childDirectory in Directory.GetDirectories(currDirectory))
                {
                    string filePath = childDirectory + @"\" + Path.GetFileName(path);
                    if (File.Exists(filePath)) return filePath;
                    
                    for (int i = 1; i < folders.Length; i++) //Ignore the root
                    {
                        var child = childDirectory + @"\" + folders[i];
                        if (Directory.Exists(child))
                        {
                            return TryLocatePath(child + @"\" + Path.GetFileName(path),folders, exclude);
                        }
                    }
                }
            }

            if (Directory.GetParent(currDirectory) == null) return null;
            
            return TryLocatePath( Directory.GetParent( currDirectory ).FullName + @"\" + Path.GetFileName( path ), folders, exclude );
        }


 
        public IWildFile CreateWildFile( string path, bool fromRegistry )
        {
            return new WildFile(path, fromRegistry);
        }

        public IWildFile CreateWildFile( string filename )
        {
            return new WildFile( filename );
        }
    }
}
