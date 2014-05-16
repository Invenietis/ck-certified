using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Plugin;

namespace CommonServices
{
    [Plugin( SharedConfigPlugin.PluginIdString, PublicName = PluginPublicName, Version = SharedConfigPlugin.PluginIdVersion,
          Categories = new string[] { "Configuration" } )]
    public class SharedConfigPlugin : IPlugin
    {
        const string PluginPublicName = "SharedConfigPlugin";
        const string PluginIdString = "{00000000-0000-0000-0000-000000000000}";
        const string PluginIdVersion = "1.0.0";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        #region IPlugin Members

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

        #endregion
    }
}
