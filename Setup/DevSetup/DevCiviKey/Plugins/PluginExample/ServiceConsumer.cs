using System;
using System.Collections.Generic;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using System.Windows;
using DummyPlugins;
using System.Diagnostics;

namespace Plugin
{
    /// <summary>
    /// Class that represent a CiviKey plugin, in order to explain how to use a CiviKeyService it consumes the IDummyService implemented in the DummyServiceProvider Project
    /// </summary>
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class ServiceConsumer : IPlugin
    {
        //This GUID should be re-generated to give this plugin a unique ID
        const string PluginGuidString = "{9F01F364-008C-4f1f-831B-CD4FF7A2D1CC}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "ServiceConsumer";

        //Reference to the storage object that enables one to save data.
        //This object is injected after all plugins' Setup method has been called
        public IPluginConfigAccessor Config { get; set; }

        //These two lines show how to use a CiviKeyService.
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IDummyService DummyService { get; set; }

        /// <summary>
        /// Constructor of the class, all services are null
        /// </summary>
        public ServiceConsumer()
        {
            w = new Window();
        }

        Window w;

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
            w.Content = String.Format( "This is a new plugin, using the DummyService. The answer to life is : {0}", DummyService.GetAnswerToLife() );
            w.Show();
        }

        /// <summary>
        /// First method called when the plugin is stopping
        /// You should remove all references to any service here.
        /// </summary>
        public void Stop()
        {
            w.Close();
        }

        /// <summary>
        /// Called after Stop()
        /// All services are null
        /// </summary>
        public void Teardown()
        {
        }
    }
}
