using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.Plugin;

namespace Prediction
{
    [Plugin( PredictionPlugin.PluginIdString, PublicName = PluginPublicName, Version = PredictionPlugin.PluginIdVersion, Categories = new string[] { "Visual", "Accessibility" } )]
    public class PredictionPlugin : IPlugin
    {
        #region Plugin description

        const string PluginPublicName = "Prediction Plugin";
        const string PluginIdString = "{C78A5CC8-449F-4A73-88B4-A8CDC3D88534}";
        const string PluginIdVersion = "1.0.0";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        #endregion

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Teardown()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
