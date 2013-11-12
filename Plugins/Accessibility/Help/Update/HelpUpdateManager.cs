using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CK.Context;
using CK.Core;
using CK.Plugin;
using Help.Services;
using Host.Services;

namespace Help.UpdateManager
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class HelpUpdateManager : IPlugin
    {
        const string PluginGuidString = "{DC7F6FC8-EA12-4FDF-8239-03B0B64C4EDE}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Help updater";
        public readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginIdVersion, PluginPublicName );

        HttpClient _http;
        HelpContentManipulator _helpContents;

        [RequiredService]
        public ISimplePluginRunner PluginRunner { get; set; }

        [RequiredService]
        public IHostInformation HostInformations { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        // Since the IHostHelp implementation is pushed to the servicecontainer after plugins are discovered and loaded, 
        // we cant use the RequiredService tag to fetch a ref to the IHostHelp.
        IHostHelp _hostHelp;
        public IHostHelp HostHelp { get { return _hostHelp ?? (_hostHelp = Context.ServiceContainer.GetService<IHostHelp>()); } }

        public bool Setup( IPluginSetupInfo info )
        {
            _http = new HttpClient();
            return true;
        }

        public void Start()
        {
            _helpContents = new HelpContentManipulator( HostInformations );
            PluginRunner.ApplyDone += OnPluginRunnerApplyDone;

            _helpContents.FindOrCreateBaseContent();
            RegisterAllAvailableDefaultHelpContents( includeHost: true );

            AutoUpdate();
        }


        public void Stop()
        {
            PluginRunner.ApplyDone -= OnPluginRunnerApplyDone;
        }

        public void Teardown()
        {
            _http.Dispose();
        }

        #region Auto register marked plugins

        void OnPluginRunnerApplyDone( object sender, ApplyDoneEventArgs e )
        {
            if( e.Success )
                RegisterAllAvailableDefaultHelpContents();
        }

        void RegisterAllAvailableDefaultHelpContents( bool includeHost = false )
        {
            IEnumerable<IPluginProxy> inspectablePlugins = PluginRunner.PluginHost.LoadedPlugins
                                                                .Where( IsHelpablePlugin );

            foreach( var p in inspectablePlugins )
            {
                IHaveDefaultHelp helpP = p.RealPluginObject as IHaveDefaultHelp;
                _helpContents.RegisterHelpContent( p, helpP.GetDefaultHelp );
            }

            if( includeHost )
            {
                _helpContents.RegisterHelpContent( HostHelp.FakeHostHelpId, HostHelp.GetDefaultHelp );
            }

        }

        bool IsHelpablePlugin( IPluginProxy pluginProxy )
        {
            return pluginProxy.Status == InternalRunningStatus.Started
                && typeof( IHaveDefaultHelp ).IsAssignableFrom( pluginProxy.RealPluginObject.GetType() );
        }

        #endregion

        #region Update management

        /// <summary>
        /// Automatically update help contents of currently started plugin when it's possible
        /// </summary>
        public void AutoUpdate()
        {
            int maxParalleleRequests = 5;
            int currentRequestCount = 0;
            IList<IPluginProxy> pluginsToProcess = PluginRunner.PluginHost.LoadedPlugins
                                                        .Where( p => p.Status == InternalRunningStatus.Started ).ToList();

            List<Task> currentTasks = new List<Task>(5);

            while( currentRequestCount < pluginsToProcess.Count )
            {
                currentTasks.Clear();

                for( int i = currentRequestCount; i < maxParalleleRequests; i++ )
                {
                    if( i == pluginsToProcess.Count ) break;

                    var currentPlugin = pluginsToProcess[i];
                    var checkTask = CheckForUpdate( currentPlugin );
                    checkTask.ContinueWith( u =>
                    {
                        if( u.Result )
                        {
                            var dlTask = DownloadUpdate( currentPlugin );
                            dlTask.ContinueWith( t => InstallUpdate( currentPlugin, t.Result ) );
                            dlTask.Start();
                        }
                    } );

                    currentTasks.Add( checkTask );
                    checkTask.Start();
                }
                Task.WaitAll( currentTasks.ToArray() );
            }
        }

        Task<bool> CheckForUpdate( IVersionedUniqueId plugin )
        {
            // lookup and found the help hash
            // create the update url to request
            // start the request and return the task

            return _http.GetAsync( "http://api.civikey.local/v2/help/pluginid/1.0.0/fr-FR/HASH/isupdated" ).ContinueWith( r =>
            {
                var res = r.Result;
                return false;
            } );
        }

        Task<TemporaryFile> DownloadUpdate( IVersionedUniqueId plugin )
        {
            // create the download url
            // start the request
            // and continue with

            throw new NotImplementedException();
        }

        void InstallUpdate( IVersionedUniqueId plugin, TemporaryFile file, bool force = false )
        {
            // analyse the file
            // if !force, then check if it can be safely installed
        }

        #endregion
    }
}
