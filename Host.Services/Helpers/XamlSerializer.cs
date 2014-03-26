#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host.Services\Helpers\XamlSerializer.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.IO;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CK.Storage;
using System.Drawing;
using System.Drawing.Imaging;

namespace Host.Services.Helper
{
    public class XamlSerializer<T> : IStructuredSerializer<T>
    {
        public object ReadInlineContent( IStructuredReader sr, T o )
        {
            return (T)XamlReader.Parse( sr.Xml.ReadInnerXml() );
        }

        public void WriteInlineContent( IStructuredWriter sw, T o )
        {
            XamlWriter.Save( o, sw.Xml );
        }
    }


    public class BitmapSourceSerializer<T> : IStructuredSerializer<T> where T : BitmapSource
    {
        public object ReadInlineContent( IStructuredReader sr, T o )
        {
            byte[] buffer = new byte[256000];
            int len  = sr.Xml.ReadElementContentAsBase64( buffer, 0, buffer.Length);
            MemoryStream stream = new MemoryStream( buffer, 0, len );

            BitmapImage bi = new BitmapImage();
            stream.Seek( 0, SeekOrigin.Begin );
            
            //Set the transparency
            Bitmap i =(Bitmap) Bitmap.FromStream( stream );
            stream.Seek( 0, SeekOrigin.Begin );
            i.MakeTransparent(System.Drawing.Color.Black);
            i.Save( stream, ImageFormat.Png );
            stream.Seek( 0, SeekOrigin.Begin );
            
            //init teh bitmapimage with the stream
            bi.BeginInit();
            bi.StreamSource = stream;
            bi.EndInit();
                    
            return (BitmapSource) bi; 
        }

        public void WriteInlineContent( IStructuredWriter sw, T o )
        {
            MemoryStream stream = new MemoryStream();
            BmpBitmapEncoder be = new BmpBitmapEncoder();
            
            be.Frames.Add( BitmapFrame.Create( (BitmapSource) o ));
            be.Save( stream );
            stream.Flush();
            byte[] buffer = stream.ToArray();
            sw.Xml.WriteBase64( buffer, 0, buffer.Length );
        }
    }

    public static class BitmapSourceExtension
    {
       
    }
}
