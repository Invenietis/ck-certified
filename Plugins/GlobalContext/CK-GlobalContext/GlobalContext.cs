using System;
using System.Collections.Generic;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CK_GlobalContext
{
    /// <summary>
    /// Class that represent a CiviKey plugin
    /// </summary>
    [Plugin(PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion)]
    public class GlobalContext : IPlugin,IGlobalContextService
    {
        //This GUID has been generated when you created the project, you may use it as is
        const string PluginGuidString = "{57b881fe-463f-43ae-964d-70d2b8e34ae1}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "CK-GlobalContext";
        Window _w;
        public ObservableCollection<string> Context { get; set; }
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
            Context = new ObservableCollection<string>();
            _w = new Window();
            _w.Content = "Global Context";
            _w.Show();
            Context.CollectionChanged += new NotifyCollectionChangedEventHandler(InventoryRecords_CollectionChanged);
        }

        /// <summary>
        /// First method called when the plugin is stopping
        /// You should remove all references to any service here.
        /// </summary>
        public void Stop()
        {
            _w.Close();
            _w = null;
        }

        /// <summary>
        /// Called after Stop()
        /// All services are null
        /// </summary>
        public void Teardown()
        {
        }
        
        void InventoryRecords_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _w.Dispatcher.Invoke(new Action(delegate()
            {
                _w.Content = "";
            }));
            IEnumerator<string> i=Context.GetEnumerator();
            while(i.MoveNext())
                _w.Dispatcher.Invoke(new Action(delegate()
                {
                    _w.Content += i.Current + "\n"; ;
                })); 
        } 
    }
}