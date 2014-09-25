#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\FileLauncher\WildFile.cs) is part of CiviKey. 
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
                SetIconFromPath();
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

        void SetIconFromPath()
        {
            Icon ico = System.Drawing.Icon.ExtractAssociatedIcon( _path );
            Bitmap bmp = System.Drawing.Icon.ExtractAssociatedIcon( _path ).ToBitmap();
            Icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
               bmp.GetHbitmap(),
               IntPtr.Zero,
               System.Windows.Int32Rect.Empty,
               BitmapSizeOptions.FromWidthAndHeight( bmp.Size.Width, bmp.Size.Height ) );
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
