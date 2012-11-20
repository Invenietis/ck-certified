using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.Engines
{
    internal class SybilleWordPredictorEngineFactory : IWordPredictorEngineFactory
    {
        Func<string> _userPath;
        Func<string> _pluginResourceDirectory;
        const string _sybileDataPath = "Data";
        IWordPredictorFeature _predictorFeature;

        public SybilleWordPredictorEngineFactory( Func<string> pluginResourceDirectory, IWordPredictorFeature predictorFeature )
            : this( pluginResourceDirectory, pluginResourceDirectory, predictorFeature )
        {
        }

        public SybilleWordPredictorEngineFactory( Func<string> pluginResourceDirectory, Func<string> userPath, IWordPredictorFeature predictorFeature )
        {
            if( pluginResourceDirectory == null ) throw new ArgumentNullException( "pluginResourceDirectory" );
            if( userPath == null ) throw new ArgumentNullException( "userPath" );
            if( predictorFeature == null ) throw new ArgumentNullException( "predictorFeature" );

            _pluginResourceDirectory = pluginResourceDirectory;
            _userPath = userPath;
            _predictorFeature = predictorFeature;
        }

        public IWordPredictorEngine Create( string predictorName )
        {
            string p = _pluginResourceDirectory();
            string userPath = _userPath();
            return DoCreate( predictorName, p, userPath, _predictorFeature );
        }

        public Task<IWordPredictorEngine> CreateAsync( string predictorName )
        {
            string p = _pluginResourceDirectory();
            string userPath = _userPath();
            return Task.Factory.StartNew<IWordPredictorEngine>( () =>
            {
                return DoCreate( predictorName, p, userPath, _predictorFeature );
            } );
        }

        private static IWordPredictorEngine DoCreate( string predictorName, string pluginResourcePath, string userPath, IWordPredictorFeature predictorFeature )
        {
            string userTextsFilePath = EnsureFileCreation( userPath, "UserTexts_fr.txt" );
            string userPredictorSibFilePath = EnsureFileCopy( Path.Combine( pluginResourcePath, _sybileDataPath ), userPath, "UserPredictor_fr.sib" );
            
            switch( predictorName.ToLowerInvariant() )
            {
                case "sybille": return new SybilleWordPredictorEngine( predictorFeature,
                    Path.Combine( pluginResourcePath, _sybileDataPath, "LMbin_fr.sib" ),
                    userPredictorSibFilePath,
                    userTextsFilePath );
                case "sem-sybille": return new SybilleWordPredictorEngine( predictorFeature,
                    Path.Combine( pluginResourcePath, _sybileDataPath, "LMbin_fr.sib" ),
                    userPredictorSibFilePath,
                    userTextsFilePath,
                    Path.Combine( pluginResourcePath, _sybileDataPath, "SemMatrix_fr.bin" ),
                    Path.Combine( pluginResourcePath, _sybileDataPath, "SemWords_fr.txt" ),
                    Path.Combine( pluginResourcePath, _sybileDataPath, "SemLambdas_fr.txt" ) );
                default: throw new ArgumentException( String.Format( "There is no word predictor engine for the name: {0}", predictorName ), "predictorName" );
            }
        }

        const string sybilleSubDirectoryName =  "Sybille";

        private static string EnsureFileCopy( string sourcePath, string destDirectoryPath, string fileName )
        {
            string destFilePath = Path.Combine( destDirectoryPath, sybilleSubDirectoryName, fileName );

            if( !File.Exists( destFilePath ) )
            {
                string sourceFilePath = Path.Combine( sourcePath, fileName );
                File.Copy( sourceFilePath, destFilePath );
            }
            return destFilePath;
        }

        private static string EnsureFileCreation( string userPath, string fileName )
        {
            string path = Path.Combine( userPath, sybilleSubDirectoryName, fileName );
            if( File.Exists( path ) ) return path;

            string sybilleUserDirectory = Path.Combine( userPath, sybilleSubDirectoryName );
            if( !Directory.Exists( sybilleUserDirectory ) ) Directory.CreateDirectory( sybilleUserDirectory );

            File.Create( path ).Close();
            return path;
        }

        public void Release( IWordPredictorEngine engine )
        {
            var disposable = engine as IDisposable;
            if( disposable != null )
            {
                disposable.Dispose();
            }
        }
    }
}
