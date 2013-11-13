using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CK.Context;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using Common.Logging;
using Help.Services;
using Host.Services;
using Ionic.Zip;

namespace Help.UpdateManager
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class HelpUpdateManager : IPlugin
    {
        const string PluginGuidString = "{DC7F6FC8-EA12-4FDF-8239-03B0B64C4EDE}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Help updater";
        public readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginIdVersion, PluginPublicName );

        static ILog _log = LogManager.GetLogger( typeof( HelpUpdateManager ) );

        HttpClient _http;
        HelpContentManipulator _helpContents;

        [RequiredService]
        public ISimplePluginRunner PluginRunner { get; set; }

        [RequiredService]
        public IHostInformation HostInformations { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        // Since the IHostHelp implementation is pushed to the servicecontainer after plugins are discovered and loaded, 
        // we cant use the RequiredService tag to fetch a ref to the IHostHelp.
        IHostHelp _hostHelp;
        public IHostHelp HostHelp { get { return _hostHelp ?? (_hostHelp = Context.ServiceContainer.GetService<IHostHelp>()); } }

        public IPluginConfigAccessor Configuration { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            _http = new HttpClient();
            return true;
        }

        public void Start()
        {
            HelpServerUrl = Configuration.System.GetOrSet( "HelpServerUrl", "http://api.civikey.invenietis.com/" );
            if( !HelpServerUrl.EndsWith( "/" ) ) HelpServerUrl += "/";

            _helpContents = new HelpContentManipulator( HostInformations );
            PluginRunner.ApplyDone += OnPluginRunnerApplyDone;

            _helpContents.FindOrCreateBaseContent();

            RegisterAllAvailableDefaultHelpContentsAsync();
            AutoUpdateAsync();
        }


        public void Stop()
        {
            PluginRunner.ApplyDone -= OnPluginRunnerApplyDone;
        }

        public void Teardown()
        {
            _http.Dispose();
        }

        string HelpServerUrl { get; set; }


        void OnPluginRunnerApplyDone( object sender, ApplyDoneEventArgs e )
        {
            if( e.Success )
            {
                RegisterAllAvailableDefaultHelpContentsAsync();
                AutoUpdateAsync();
            }
        }

        #region Auto register marked plugins

        Task RegisterAllAvailableDefaultHelpContentsAsync()
        {
            IEnumerable<IPluginProxy> inspectablePlugins = PluginRunner.PluginHost.LoadedPlugins
                                                                .Where( IsHelpablePlugin );
            return Task.Factory.StartNew( () =>
            {
                foreach( var p in inspectablePlugins )
                {
                    IHaveDefaultHelp helpP = p.RealPluginObject as IHaveDefaultHelp;
                    _helpContents.FindOrCreateDefaultContent( p, helpP.GetDefaultHelp );
                }

                _helpContents.FindOrCreateDefaultContent( HostHelp.FakeHostHelpId, HostHelp.GetDefaultHelp );
            } );
        }

        bool IsHelpablePlugin( IPluginProxy pluginProxy )
        {
            return pluginProxy.Status == InternalRunningStatus.Started
                && typeof( IHaveDefaultHelp ).IsAssignableFrom( pluginProxy.RealPluginObject.GetType() );
        }

        #endregion

        #region Update management

        /// <summary>
        /// Automatically update help contents of currently started plugin when it's possible
        /// </summary>
        Task AutoUpdateAsync()
        {
            IList<IVersionedUniqueId> pluginsToProcess =  PluginRunner.PluginHost.LoadedPlugins.Cast<IVersionedUniqueId>().ToList();
            pluginsToProcess.Add( HostHelp.FakeHostHelpId );

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5 };

            return Task.Factory.StartNew( () => Parallel.ForEach( pluginsToProcess, parallelOptions, item => AutoUpdateAsync( item ).Wait() ) );
        }

        Task AutoUpdateAsync( IVersionedUniqueId plugin )
        {
            return CheckForUpdate( plugin )
                .ContinueWith( u =>
                {
                    if( u.Result )
                    {
                        DownloadUpdate( plugin )
                            .ContinueWith( t => InstallUpdate( plugin, t.Result ) )
                            .Wait();
                    }
                } );
        }

        Task<bool> CheckForUpdate( IVersionedUniqueId plugin )
        {
            // lookup and found the help hash
            string hash = "HASHNOTFOUND";
            var helpIndex = _helpContents.GetHelpContentFilePath( plugin );
            if( helpIndex != _helpContents.NoContentFilePath )
            {
                var hashFile = new FileInfo( helpIndex ).Directory.EnumerateFiles( "hash" ).FirstOrDefault();
                if( hashFile != null )
                {
                    using( var rdr = hashFile.OpenText() )
                        hash = rdr.ReadLine();
                }
            }

            // create the update url to request
            string url = string.Format( "{0}/v2/help/{1}/{2}/{3}/{4}/isupdated", HelpServerUrl, plugin.UniqueId.ToString( "B" ), plugin.Version.ToString(), CultureInfo.CurrentCulture.TextInfo.CultureName, hash );

            // start the request and return the task
            return _http.GetAsync( url ).ContinueWith( u =>
            {
                // parse the result to return if the plugin has a new help content that we have to download
                bool result = false;
                if( u.Result.StatusCode == System.Net.HttpStatusCode.OK )
                {
                    string rawresult = u.Result.Content.ReadAsStringAsync().Result;
                    bool.TryParse( rawresult, out result );
                }
                return result;
            } );
        }

        Task<TemporaryFile> DownloadUpdate( IVersionedUniqueId plugin )
        {
            // create the download url
            string url = string.Format( "{0}/v2/help/{1}/{2}/{3}", HelpServerUrl, plugin.UniqueId.ToString( "B" ), plugin.Version.ToString(), CultureInfo.CurrentCulture.TextInfo.CultureName );

            // start the request
            // and continue with
            return _http.GetStreamAsync( url ).ContinueWith( t =>
            {
                var tempFile = new TemporaryFile( ".zip" );
                using( var s = File.OpenWrite( tempFile.Path ) )
                    t.Result.CopyTo( s );
                return tempFile;
            } );
        }

        void InstallUpdate( IVersionedUniqueId plugin, TemporaryFile file, bool force = false )
        {
            HelpManifestData manifest = null;

            try
            {
                // open the zip file
                using( ZipFile zip = ZipFile.Read( file.Path ) )
                {
                    var manifestEntry = zip.Where( z => z.FileName == "manifest.xml" ).FirstOrDefault();
                    if( manifestEntry != null )
                    {
                        var ms = new MemoryStream();
                        manifestEntry.Extract( ms );
                        try
                        {
                            manifest = HelpManifestData.Deserialize( ms );
                        }
                        catch( Exception ex )
                        {
                            _log.Error( "Unable to parse manifest", ex );
                            return;
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                _log.Error( "Unable to read downloaded help content as a zip file", ex );
                return;
            }

            if( force || IsInstallationPossibleAutomatically( plugin, manifest ) )
            {
                SimpleVersionedUniqueId pluginIdBasedOnManifest = new SimpleVersionedUniqueId( manifest.PluginId, manifest.Version );
                _helpContents.InstallDownloadedHelpContent( pluginIdBasedOnManifest, () => File.OpenRead( file.Path ), manifest.Culture );
            }

        }

        bool IsInstallationPossibleAutomatically( IVersionedUniqueId plugin, HelpManifestData manifest )
        {
            Version manifestVersion = new Version(manifest.Version);

            Debug.Assert( plugin.UniqueId.ToString( "B" ) == manifest.PluginId );
            Debug.Assert( plugin.Version >= manifestVersion );

            var currentHelpFile = new FileInfo( _helpContents.GetHelpContentFilePath( plugin, CultureInfo.CurrentCulture.TextInfo.CultureName ) );
            if( currentHelpFile.FullName != _helpContents.NoContentFilePath )
            {
                string currentCulture = currentHelpFile.Directory.Name;
                Version currentVersion = new Version( currentHelpFile.Directory.Parent.Name );

                // if the manifest version is lesser than the current version we shouldn't install the help content
                // otherwise we can delete a better local version
                if( currentVersion <= manifestVersion )
                {
                    return manifest.Culture == CultureInfo.CurrentCulture.TextInfo.CultureName
                        || manifest.Culture == CultureInfo.CurrentCulture.TwoLetterISOLanguageName
                        || manifest.Culture == currentCulture;
                }

                return false;
            }
            else
                return true; // by default if there is no local help, use the remote one it's better than nothing
        }

        #endregion
    }
}
