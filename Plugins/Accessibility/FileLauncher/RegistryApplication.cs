using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace FileLauncher
{
    public class RegistryApplication
    {
        public static List<FileInfo> Crawl()
        {
            List<FileInfo> list = new List<FileInfo>();
            var r = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths");
            var vals = r.GetSubKeyNames();

            foreach (string s in vals)
            {
                string path = r.OpenSubKey(s).GetValue("", null) as string;
                if (path != null && File.Exists(path)) list.Add(new FileInfo(path));
            }

            return list;
        }

        public static void Launch(FileInfo fInfo)
        {
            Process.Start(fInfo.Path); 
        }
    }
}
