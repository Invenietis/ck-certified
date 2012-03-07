using System;
using System.Collections.Generic;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using System.Windows;

namespace DummyPlugins
{
    /// <summary>
    /// Class that represent a CiviKey service. It is a plugin that implements a CiviKeyService (here, IDummyService)
    /// </summary>
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class DummyServicePlugin : IPlugin, IDummyService
    {
        //This GUID should be re-generated to give this plugin a unique ID
        const string PluginGuidString = "{8C4DA5A4-95E5-4a9e-83BF-51FFC0B43E59}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "DummyServicePlugin";

        //Reference to the storage object that enables one to save data.
        //This object is injected after all plugins' Setup method have been called
        public IPluginConfigAccessor Config { get; set; }

        /// <summary>
        /// Constructor of the class, all services are null
        /// </summary>
        public DummyServicePlugin()
        {
        }

        /// <summary>
        /// First called method on the class, at this point, all services are null.
        /// Used to set up the service exposed by this plugin (if any).
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        /// <summary>
        /// Called after the Setup method.
        /// All launched services are now set, you may now set up the link to the service used by this class
        /// </summary>
        public void Start()
        {
            //Saving a value into the SharedDictionary. Note that these values are saved as XML when CiviKey is stopped.
            if( Config.User["AnswerToLife"] == null)
                Config.User.Add( "AnswerToLife", 42 );
        }

        /// <summary>
        /// First method called when the plugin is stopping
        /// You should remove all references to any service here.
        /// </summary>
        public void Stop()
        {

        }

        /// <summary>
        /// Called after Stop()
        /// All services are null
        /// </summary>
        public void Teardown()
        {
        }

        public int GetAnswerToLife()
        {
            //Retrieve values from the SharedDictionary
            return (int)Config.User["AnswerToLife"];
        }
    }
}
