using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace FileLauncher
{
    public class FileInfo
    {
        public string FileName { get; private set; }
        public string Path { get; private set; }
        public string AppName { get; private set; }
        public Icon Icon { get; private set; }

        public FileInfo(string path)
        {
            Path = path;
            Console.WriteLine("File : " + path);
            FileVersionInfo finfo = FileVersionInfo.GetVersionInfo(path);
            FileName = System.IO.Path.GetFileName(Path);
            AppName = finfo.ProductName;
            Icon = Icon.ExtractAssociatedIcon(path);
        }
    }
}
