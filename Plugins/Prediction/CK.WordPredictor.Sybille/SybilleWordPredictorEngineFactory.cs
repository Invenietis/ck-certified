using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CK.Plugin;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.Engines
{
    internal class SybilleWordPredictorEngineFactory : IWordPredictorEngineFactory
    {
        Func<string> _userPath;
        Func<string> _pluginResourceDirectory;
        const string _sybileDataPath = "Data";
        IService<IWordPredictorFeature> _predictorFeature;

        public SybilleWordPredictorEngineFactory( Func<string> pluginResourceDirectory, IService<IWordPredictorFeature> predictorFeature )
            : this( pluginResourceDirectory, pluginResourceDirectory, predictorFeature )
        {
        }

        public SybilleWordPredictorEngineFactory( Func<string> pluginResourceDirectory, Func<string> userPath, IService<IWordPredictorFeature> predictorFeature )
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

        Task<IWordPredictorEngine> _currentlyRunningTask;
        CancellationTokenSource cancellationSource = null;

        public Task<IWordPredictorEngine> CreateAsync( string predictorName )
        {
            //if the last is still running 
            if( _currentlyRunningTask != null && _currentlyRunningTask.Status <= TaskStatus.Running )
            {
                Debug.Assert( cancellationSource != null );
                cancellationSource.Cancel();
                //_currentlyRunningTask.Wait( cancellationSource.Token );
                cancellationSource.Dispose();
                cancellationSource = new CancellationTokenSource();
            }

            if( cancellationSource == null )
                cancellationSource = new CancellationTokenSource();

            string p = _pluginResourceDirectory();
            string userPath = _userPath();

            _currentlyRunningTask = Task.Factory.StartNew<IWordPredictorEngine>( () =>
            {
                if( cancellationSource.IsCancellationRequested )
                    return null;
                SybilleWordPredictorEngine engine = DoCreate( predictorName, p, userPath, _predictorFeature );
                if( engine.ConstructionSuccess ) return engine;
                return null;
            }, cancellationSource.Token );

            return _currentlyRunningTask;
        }

        private static SybilleWordPredictorEngine DoCreate( string predictorName, string pluginResourcePath, string userPath, IService<IWordPredictorFeature> predictorFeature )
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
