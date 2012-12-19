using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Context;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices.Accessibility;

namespace OnlineHelp
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class OnlineHelp : IPlugin, IHelpService
    {
        const string PluginGuidString = "{1DB78D66-B5EC-43AC-828C-CCAB91FA6210}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "OnlineHelp";
        public readonly INamedVersionedUniqueId PluginId;

        [RequiredService]
        public IHostInformation HostInformations { get; set; }

        private string LocalHelpBaseDirectory { get { return HostInformations.ApplicationDataPath; } }

        string GetHelpContentFilePath( INamedVersionedUniqueId pluginName )
        {
            string localhelp = Path.Combine( LocalHelpBaseDirectory, pluginName.UniqueId.ToString( "B" ), pluginName.Version.ToString(), "index.html" );
            if( !File.Exists( localhelp ) )
            {
                localhelp = Path.Combine( LocalHelpBaseDirectory, "Default", "nocontent.html" );
            }

            return localhelp;
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
        }

        public CancellationTokenSource GetHelpFor( INamedVersionedUniqueId pluginName, Action<Task<string>> onComplete )
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            Task<string> getContent = new Task<string>( () =>
            {
                using( StreamReader rdr = new StreamReader( GetHelpContentFilePath( pluginName ) ) )
                {
                    return rdr.ReadToEnd();
                }
            }, cancellationToken.Token );

            getContent.ContinueWith( onComplete, cancellationToken.Token );

            getContent.Start();

            return cancellationToken;
        }
    }
}
