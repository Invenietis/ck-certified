﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommonServices;

namespace FileLauncher
{
    public class WildFile : IWildFile
    {
        string _path;
        public FileLookup Lookup { get; internal set; }
        public Environment.SpecialFolder? FolderLocationType { get; internal set; }
        public string FileName { get; private set; }
        public DateTime LastAccessTime { get; private set; }

        public string Path 
        {
            get { return _path; } 
            internal set
            {
                _path = value;
                if( !File.Exists( _path )) return;
                
                Bitmap bmp = System.Drawing.Icon.ExtractAssociatedIcon(_path).ToBitmap();
                Icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmp.GetHbitmap(),
                   IntPtr.Zero,
                   System.Windows.Int32Rect.Empty,
                   BitmapSizeOptions.FromWidthAndHeight(bmp.Size.Width, bmp.Size.Height));

                LastAccessTime = File.GetLastAccessTime( _path );
            }
        }

        public ImageSource Icon { get; set; }
        public bool IsLocated 
        { 
            get { return Lookup == FileLookup.Url && Uri.IsWellFormedUriString(Path, UriKind.Absolute) || File.Exists(Path); }
        }

        public WildFile(string name)
        {
            FileName = name;
            _path = "";
        }

        public WildFile( string path, bool fromRegistry )
            : this( System.IO.Path.GetFileName( path ) )
        {
            Path = path;
            FolderLocationType = FileLocator.GetSpecialFolder( path );

            Lookup = fromRegistry ? FileLookup.Registry : FileLookup.SpecialFolder;

            if( FolderLocationType == null && !fromRegistry )
            {
                if(Uri.IsWellFormedUriString(Path, UriKind.Absolute))
                {
                    Lookup = FileLookup.Url;
                }
                else
                {
                    Lookup = FileLookup.Other;
                }
            }
        }

        #region IComparable Members

        public int CompareTo( object obj )
        {
            if(obj is IWildFile)
            {
                IWildFile wf = obj as IWildFile;
                if( wf.FileName == this.FileName && wf.Lookup == this.Lookup ) return 0;
            }
            
            return -1;
        }

        #endregion
    }

    public class WildApp : WildFile
    {
        FileVersionInfo Info 
        {
            get { return FileVersionInfo.GetVersionInfo(Path); } 
        }

        public WildApp(string name) : base(name) { }
    }

}
