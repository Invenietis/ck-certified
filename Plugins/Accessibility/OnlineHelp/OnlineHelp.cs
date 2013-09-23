using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CK.Context;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices.Accessibility;
using Host.Services;
using Ionic.Zip;

namespace OnlineHelp
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class OnlineHelp : IPlugin, IHelpService
    {
        const string PluginGuidString = "{1DB78D66-B5EC-43AC-828C-CCAB91FA6210}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.1";
        const string PluginPublicName = "OnlineHelp";
        public readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginIdVersion, PluginPublicName );

        public string _defaultCulture = "en";
        HelpBrowser _helpBrowser;

        [RequiredService]
        public IHostInformation HostInformations { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        //Since the IHostHelp implementation is pushed to the servicecontainer after plugins are discovered and loaded, we cant use the RequiredService tag to fetch a ref to the IHostHelp.
        /// <summary>
        /// The HostManipulator, enables minimizing the host.
        /// </summary>
        IHostHelp _hostHelp;
        public IHostHelp HostHelp { get { return _hostHelp ?? ( _hostHelp = Context.ServiceContainer.GetService<IHostHelp>() ); } }

        public IPluginConfigAccessor Config { get; set; }

        List<string> _registeredHelps;
        List<string> RegisteredHelps
        {
            get
            {
                return _registeredHelps ?? ( _registeredHelps = Config.User.GetOrSet( "registeredHelps", new List<string>() ) );
            }
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            HostHelp.ShowHostHelp += ( o, e ) =>
            {
                RegisterHelpContent( e.HostUniqueId, typeof( OnlineHelp ).Assembly.GetManifestResourceStream( "OnlineHelp.Res.hosthelpcontent.zip" ) );
                ShowHelpFor( e.HostUniqueId, true );
            };
            if( !RegisteredHelps.Contains( "default" ) )
            {
                UnzipAndExtractStream( typeof( OnlineHelp ).Assembly.GetManifestResourceStream( "OnlineHelp.Res.helpbase.zip" ), LocalHelpBaseDirectory );
                RegisteredHelps.Add( "default" );
            }
        }

        public void Stop()
        {
            if( _helpBrowser != null ) _helpBrowser.Close();
        }

        public void Teardown()
        {
        }

        public CancellationTokenSource GetHelpContentFor( IVersionedUniqueId pluginName, Action<Task<string>> onComplete )
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            Task<string> getContent = new Task<string>( () =>
            {
                using( StreamReader rdr = new StreamReader( GetHelpLocalContentFilePath( pluginName ) ) )
                {
                    return rdr.ReadToEnd();
                }
            }, cancellationToken.Token );

            getContent.ContinueWith( onComplete, cancellationToken.Token );

            getContent.Start();

            return cancellationToken;
        }

        public bool ShowHelpFor( IVersionedUniqueId pluginName, bool force = false )
        {
            string url = GetHelpLocalContentFilePath( pluginName );
            bool found = url != NoContentFilePath;
            if( found || force )
            {
                Application.Current.Dispatcher.BeginInvoke( (Action)( () =>
                {
                    HelpBrowser._browser.Navigate( url );
                    HelpBrowser.Show();
                } ), null );
            }
            return found;
        }

        public void RegisterHelpContent( IVersionedUniqueId pluginName, Stream zipContent )
        {
            string key = pluginName.UniqueId.ToString( "B" ) + pluginName.Version.ToString();
            if( !RegisteredHelps.Contains( key ) )
            {
                UnzipAndExtractStream( zipContent, GetBaseHelpDirectoryForPlugin( pluginName ) );
                RegisteredHelps.Add( key );
            }
        }

        private string LocalHelpBaseDirectory { get { return Path.Combine( HostInformations.ApplicationDataPath, "HelpContents" ); } }

        private string NoContentFilePath { get { return Path.Combine( LocalHelpBaseDirectory, "Default", "nocontent.html" ); } }

        private HelpBrowser HelpBrowser
        {
            get
            {
                if( _helpBrowser == null )
                {
                    _helpBrowser = new HelpBrowser();
                    _helpBrowser.Closed += OnHelpBrowserClosed;
                }
                return _helpBrowser;
            }
        }

        void OnHelpBrowserClosed( object sender, EventArgs e )
        {
            _helpBrowser.Closed -= OnHelpBrowserClosed;
            _helpBrowser = null;
        }

        string GetBaseHelpDirectoryForPlugin( IVersionedUniqueId pluginName )
        {
            return Path.Combine( LocalHelpBaseDirectory, pluginName.UniqueId.ToString( "B" ), pluginName.Version.ToString() );
        }

        string GetHelpLocalContentFilePath( IVersionedUniqueId pluginName, string culture = null )
        {
            if( culture == null ) culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant();

            // try to load the help of the plugin in the good culture (current or given)
            string localhelp = Path.Combine( GetBaseHelpDirectoryForPlugin( pluginName ), culture, "index.html" );
            if( !File.Exists( localhelp ) )
            {
                // if the help does not exists, and if the given culture is already the default, there is no help content
                if( culture == _defaultCulture )
                {
                    localhelp = NoContentFilePath;
                }
                else
                {
                    // if the given culture is still not the default, and a specialized culture, try to load the help for the base culture
                    if( culture.Contains( '-' ) ) return GetHelpLocalContentFilePath( pluginName, culture.Substring( culture.IndexOf( '-' ) ) );
                    else return GetHelpLocalContentFilePath( pluginName, _defaultCulture );
                }

            }

            return localhelp;
        }

        void UnzipAndExtractStream( Stream zipContent, string pathToExtract )
        {
            using( var zipFile = ZipFile.Read( zipContent ) )
            {
                zipFile.ExtractAll( pathToExtract, ExtractExistingFileAction.OverwriteSilently );
            }
            zipContent.Dispose();
        }
    }
}
