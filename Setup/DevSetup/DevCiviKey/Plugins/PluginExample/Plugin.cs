using System;
using System.Collections.Generic;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using System.Windows;

namespace Plugin
{
    /// <summary>
    /// Class that represent a CiviKey plugin
    /// </summary>
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class Plugin : IPlugin
    {
        //This GUID should be re-generated to give this plugin a unique ID
        const string PluginGuidString = "{9F01F364-008C-4f1f-831B-CD4FF7A2D1CC}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Name of the plugin";

        //Reference to the storage object that enables one to save data.
        //This object is injected after all plugins' Setup method has been called
        public IPluginConfigAccessor Config { get; set; }

        /// <summary>
        /// Constructor of the class, all services are null
        /// </summary>
        public Plugin()
        {
            w = new Window();
            w.Content = "This is a new plugin";
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
