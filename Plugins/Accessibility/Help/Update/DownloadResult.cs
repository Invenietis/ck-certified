using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CK.Core;
using Common.Logging;
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

        public DownloadResult( ILog logger, Stream zipContent )
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
                            logger.Error( "Unable to parse manifest", ex );
                            throw;
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                logger.Error( "Unable to read downloaded help content as a zip file", ex );
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
