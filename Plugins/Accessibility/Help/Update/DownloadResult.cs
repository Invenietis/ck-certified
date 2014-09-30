#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\Help\Update\DownloadResult.cs) is part of CiviKey. 
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
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CK.Core;
using Help.Services;
using Ionic.Zip;

namespace Help.Update
{
    public class DownloadResult : IDownloadResult
    {
        internal HelpManifestData Manifest { get; set; }

        public TemporaryFile File { get; private set; }

        public string Culture { get { return Manifest.Culture; } }

        public string Version { get { return Manifest.Version; } }

        public DownloadResult( IActivityMonitor logger, Stream zipContent )
        {
            File = new TemporaryFile( ".zip" );
            using( zipContent )
            using( var sw = System.IO.File.Open( File.Path, FileMode.Open ) )
                zipContent.CopyTo( sw );

            try
            {
                // open the zip file
                using( ZipFile zip = ZipFile.Read( File.Path ) )
                {
                    var manifestEntry = zip.Where( z => z.FileName == "manifest.xml" ).FirstOrDefault();
                    if( manifestEntry != null )
                    {
                        var ms = new MemoryStream();
                        manifestEntry.Extract( ms );
                        try
                        {
                            Manifest = HelpManifestData.Deserialize( ms );
                        }
                        catch( Exception ex )
                        {
                            logger.Error().Send(ex, "Unable to parse manifest" );
                            throw;
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                logger.Error().Send( ex, "Unable to read downloaded help content as a zip file" );
                throw;
            }
        }

        public class HelpManifestData
        {
            public string PluginId { get; set; }

            public string Version { get; set; }

            public string Culture { get; set; }

            public string Hash { get; set; }

            public static HelpManifestData Deserialize( Stream data )
            {
                data.Seek( 0, SeekOrigin.Begin );
                XmlSerializer x = new XmlSerializer( typeof( HelpManifestData ) );
                return x.Deserialize( data ) as HelpManifestData;
            }

            public void Serialize( TextWriter data )
            {
                XmlSerializer x = new XmlSerializer( typeof( HelpManifestData ) );
                x.Serialize( data, this );
            }
        }
    }
}
