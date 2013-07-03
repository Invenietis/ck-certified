using System;
using CK.Plugin;

namespace TurboScroll
{
     [Plugin( TurboScroll.PluginIdString,
           PublicName = PluginPublicName,
           Version = TurboScroll.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class TurboScroll : IPlugin
    {
        internal const string PluginIdString = "{8D6BB034-049E-4ADB-9D04-7746A071C813}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "TurboScroll";



        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            throw new NotImplementedException();
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
