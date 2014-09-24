using System;
using System.Collections.Generic;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using System.Windows;
using CK_GlobalContext;
using System.Timers;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CK_FocusedWindow
{
    /// <summary>
    /// Class that represent a CiviKey plugin
    /// </summary>
    [Plugin(PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion)]
    public class CK_FocusedWindow : IPlugin
    {
        //This GUID has been generated when you created the project, you may use it as is
        const string PluginGuidString = "{ef8e511d-b703-40e6-9d37-1f84733d4d6a}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "CK-FocusedWindow";

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        private Timer timer;
        [DynamicService(Requires = RunningRequirement.MustExistAndRun)]
        public static IGlobalContextService Context { get; set; }

        //Reference to the storage object that enables one to save data.
        //This object is injected after all plugins' Setup method has been called
        public IPluginConfigAccessor Config { get; set; }
          
        /// <summary>
        /// First called method on the class, at this point, all services are null.
        /// Used to set up the service exposed by this plugin (if any).
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Setup(IPluginSetupInfo info)
        {
            return true;
        }

        /// <summary>
        /// Called after the Setup method.
        /// All launched services are now set, you may now set up the link to the service used by this class
        /// </summary>
        public void Start()
        {
            timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Enabled = true;
            timer.Interval = 2000;
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

        public static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            IntPtr handleWindowActive = GetForegroundWindow();
            Process[] processList = Process.GetProcesses();
            
            foreach (Process process in processList)
                if (process.MainWindowHandle == handleWindowActive)
                {
                    Context.Context.Add(process.ProcessName);
                }
        }
    }
}
