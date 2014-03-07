using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CK.Context;
using CK.Core;
using Ionic.Zip;

namespace Help
{
    internal class HelpContentManipulator
    {
        const string DefaultCulture = "en";

        IHostInformation _hostInformations;

        public HelpContentManipulator( IHostInformation hostInformations )
        {
            _hostInformations = hostInformations;
        }

        public string HelpBaseDirectory { get { return Path.Combine( _hostInformations.CommonApplicationDataPath, "HelpContents" ); } }

        public string NoContentFilePath { get { return Path.Combine( HelpBaseDirectory, "Default", "nocontent.html" ); } }

        #region Read

        public string GetHelpContentFilePath( IVersionedUniqueId pluginName, string culture = null )
        {
            if( culture == null ) culture = CultureInfo.CurrentCulture.TextInfo.CultureName;

            // try to load the help of the plugin in the good culture (current or given)
            string localhelp = Path.Combine( GetBaseHelpDirectoryForPlugin( pluginName, culture ), "index.html" );
            if( !File.Exists( localhelp ) )
            {
                // if the help does not exists, and if the given culture is already the default, there is no help content
                if( culture == DefaultCulture )
                {
                    localhelp = NoContentFilePath;
                }
                else
                {
                    // if the given culture is still not the default, and a specialized culture, try to load the help for the base culture
                    if( culture.Contains( '-' ) ) return GetHelpContentFilePath( pluginName, culture.Substring( 0, culture.IndexOf( '-' ) ) );
                    else return GetHelpContentFilePath( pluginName, DefaultCulture );
                }

            }

            return localhelp;
        }

        string GetBaseHelpDirectoryForPlugin( IVersionedUniqueId pluginName, string culture = null )
        {
            string path = Path.Combine( HelpBaseDirectory, pluginName.UniqueId.ToString( "B" ), pluginName.Version == null ? "" : pluginName.Version.ToString() );
            if( !string.IsNullOrEmpty( culture ) )
            {
                path = Path.Combine( path, culture );
            }

            return path;
        }

        #endregion

        #region Write

        public void FindOrCreateBaseContent()
        {
            string baseContentPath = Path.Combine( HelpBaseDirectory, "Default" );
            if( !Directory.Exists( baseContentPath ) )
            {
                UnzipAndExtractStream( typeof( HelpContentManipulator ).Assembly.GetManifestResourceStream( "Help.Res.helpbase.zip" ), HelpBaseDirectory );
            }
        }

        public void FindOrCreateDefaultContent( IVersionedUniqueId plugin, Func<Stream> contentAccessor, string culture = null )
        {
            string pluginHelpDirectoryPath = GetBaseHelpDirectoryForPlugin( plugin, culture );
            if( !Directory.Exists( pluginHelpDirectoryPath ) )
            {
                UnzipAndExtractStream( contentAccessor(), pluginHelpDirectoryPath );
            }
        }

        public void InstallDownloadedHelpContent( IVersionedUniqueId plugin, Func<Stream> contentAccessor, string culture, bool clean = false )
        {
            DirectoryInfo pluginHelpDirectory = new DirectoryInfo( GetBaseHelpDirectoryForPlugin( plugin, culture ) );
            if( clean && pluginHelpDirectory.Parent.Exists )
            {
                pluginHelpDirectory.Parent.Delete( true );
                pluginHelpDirectory.Create();
            }

            UnzipAndExtractStream( contentAccessor(), pluginHelpDirectory.FullName, "content" );
        }

        #endregion

        #region Utility

        void UnzipAndExtractStream( Stream zipContent, string pathToExtract, string innerPathFilter = null )
        {
            using( zipContent )
            using( var zipFile = ZipFile.Read( zipContent ) )
            {
                if( string.IsNullOrEmpty( innerPathFilter ) )
                    zipFile.ExtractAll( pathToExtract, ExtractExistingFileAction.OverwriteSilently );
                else
                {
                    DirectoryInfo tempDir = new DirectoryInfo( pathToExtract ).Parent.CreateSubdirectory( Path.GetRandomFileName() );

                    if( !innerPathFilter.EndsWith( "/" ) ) innerPathFilter += "/";
                    var filesToExtract = zipFile.Where( z => z.FileName.StartsWith( innerPathFilter ) );

                    foreach( var e in filesToExtract )
                    {
                        e.Extract( tempDir.FullName, ExtractExistingFileAction.OverwriteSilently );
                    }

                    // clean the directory
                    DirectoryInfo directoryToExtract = new DirectoryInfo( pathToExtract );
                    if( directoryToExtract.Exists ) directoryToExtract.Delete( true );

                    FileUtil.CopyDirectory( new DirectoryInfo( Path.Combine( tempDir.FullName, innerPathFilter ) ), directoryToExtract );
                    tempDir.Delete( true );
                }
            }
        }

        #endregion
    }
}
