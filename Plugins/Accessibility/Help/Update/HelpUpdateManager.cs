#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\Help\Update\HelpUpdateManager.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CK.Context;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using Common.Logging;
using Help.Services;
using Help.Update.ManualUpdate;
using Host.Services;

namespace Help.Update
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class HelpUpdateManager : IPlugin, IHelpUpdaterService
    {
        const string PluginGuidString = "{DC7F6FC8-EA12-4FDF-8239-03B0B64C4EDE}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Help updater";
        public readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginIdVersion, PluginPublicName );
        Queue<IPluginProxy> _checked;
        Queue<IPluginProxy> _pending;

        static ILog _log = LogManager.GetLogger( typeof( HelpUpdateManager ) );

        string _helpServerUrl;
        //HttpClient _http;
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

        public IPluginConfigAccessor Configuration { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            //_http = new HttpClient();
            
            return true;
        }

        public void Start()
        {
            _helpServerUrl = Configuration.System.GetOrSet( "HelpServerUrl", "http://api.civikey.invenietis.com/" );
            if( !_helpServerUrl.EndsWith( "/" ) ) _helpServerUrl += "/";

            _helpContents = new HelpContentManipulator( HostInformations );
            PluginRunner.ApplyDone += OnPluginRunnerApplyDone;

            _helpContents.FindOrCreateBaseContent();
            _helpContents.FindOrCreateDefaultContent( HostHelp.FakeHostHelpId, HostHelp.GetDefaultHelp );

            _checked = new Queue<IPluginProxy>();
            _pending = new Queue<IPluginProxy>();
        }

        public void Stop()
        {
            PluginRunner.ApplyDone -= OnPluginRunnerApplyDone;
        }

        public void Teardown()
        {
            //_http.Dispose();
        }

        void OnPluginRunnerApplyDone( object sender, ApplyDoneEventArgs e )
        {
            if( e.Success )
            {
                EnqueueHelpablePlugin();
                AutoUpdateAsync();
            }
        }

        #region Auto register marked plugins

        void EnqueueHelpablePlugin()
        {
            IEnumerable<IPluginProxy> inspectablePlugins = PluginRunner.PluginHost.LoadedPlugins
                                                                .Where( IsHelpablePlugin );
            foreach( var p in inspectablePlugins )
            {
                _pending.Enqueue( p );
            }
        }
        
        bool IsQueuded(IPluginProxy pluginProxy)
        {
            return _pending.Contains( pluginProxy ) || _checked.Contains( pluginProxy );
        }

        bool IsHelpablePlugin( IPluginProxy pluginProxy )
        {
            return !IsQueuded(pluginProxy) && pluginProxy.Status == InternalRunningStatus.Started
                && typeof( IHaveDefaultHelp ).IsAssignableFrom( pluginProxy.RealPluginObject.GetType() );
        }

        #endregion

        #region Events management

        public event EventHandler<HelpUpdateEventArgs> UpdateAvailable;

        public event EventHandler<HelpUpdateDownloadedEventArgs> UpdateDownloaded;

        public event EventHandler<HelpUpdateDownloadedEventArgs> UpdateInstalled;

        void InvokeEvent<T>( EventHandler<T> eventHandler, T eventArgs )
            where T : EventArgs
        {
            Application.Current.Dispatcher.BeginInvoke( (Action)(() =>
            {
                if( eventHandler != null )
                    eventHandler( this, eventArgs );
            }) );
        }

        #endregion

        #region Update management

        /// <summary>
        /// Automatically update help contents of currently started plugin when it's possible
        /// </summary>
        void AutoUpdateAsync()
        {
            IPluginProxy plugin;

            while( _pending.Count > 0 )
            {
                plugin = _pending.Dequeue();
                IHaveDefaultHelp helpP = plugin.RealPluginObject as IHaveDefaultHelp;
                _helpContents.FindOrCreateDefaultContent( plugin, helpP.GetDefaultHelp );

                AutoUpdateAsync( plugin );
                _checked.Enqueue( plugin );
            }
        }

        void AutoUpdateAsync( INamedVersionedUniqueId plugin )
        {
            //CheckForUpdate( plugin, true )
            //    .ContinueWith( u =>
            //    {
            //        if( u.Result )
            //        {

            //            Console.WriteLine( "Check updates for" + plugin.PublicName );
            //            DownloadUpdate( plugin, true )
            //                .ContinueWith( t => AutoInstallUpdate( plugin, t.Result ) );
            //        }
            //    } );
        }

        Task ManualUpdateAsync( INamedVersionedUniqueId plugin )
        {
            return null;
            //return CheckForUpdate( plugin, false )
            //    .ContinueWith( u =>
            //    {
            //        if( u.Result )
            //        {
            //            DownloadUpdate( plugin, false )
            //                .Wait();
            //        }
            //    } );
        }

        Task<bool> CheckForUpdate( INamedVersionedUniqueId plugin, bool silent = false )
        {
            return null;
            //// lookup and found the help hash
            //string hash = "HASHNOTFOUND";
            //var helpIndex = _helpContents.GetHelpContentFilePath( plugin );
            //if( helpIndex != _helpContents.NoContentFilePath )
            //{
            //    var hashFile = new FileInfo( helpIndex ).Directory.EnumerateFiles( "hash" ).FirstOrDefault();
            //    if( hashFile != null )
            //    {
            //        using( var rdr = hashFile.OpenText() )
            //            hash = rdr.ReadLine();
            //    }
            //}

            //// create the update url to request
            //string url = string.Format( "{0}v2/help/{1}/{2}/{3}/{4}/isupdated", _helpServerUrl, plugin.UniqueId.ToString( "B" ), plugin.Version == null ? "" : plugin.Version.ToString(), CultureInfo.CurrentCulture.TextInfo.CultureName, hash );

            //// start the request and return the task
            //return _http.GetAsync( url ).ContinueWith( u =>
            //{
            //    // parse the result to return if the plugin has a new help content that we have to download
            //    bool result = false;
            //    if( u.Result.StatusCode == System.Net.HttpStatusCode.OK )
            //    {
            //        string rawresult = u.Result.Content.ReadAsStringAsync().Result;
            //        bool.TryParse( rawresult, out result );
            //    }
            //    if( result && !silent )
            //        InvokeEvent( UpdateAvailable, new HelpUpdateEventArgs( plugin ) );
            //    return result;
            //} );
        }

        Task<DownloadResult> DownloadUpdate( INamedVersionedUniqueId plugin, bool silent = false )
        {
            //// create the download url
            //string url = string.Format( "{0}v2/help/{1}/{2}/{3}", _helpServerUrl, plugin.UniqueId.ToString( "B" ), plugin.Version.ToString(), CultureInfo.CurrentCulture.TextInfo.CultureName );

            //// start the request
            //// and continue with
            //return _http.GetStreamAsync( url ).ContinueWith( t =>
            //{
            //    var r = new DownloadResult( _log, t.Result );
            //    if( !silent )
            //        InvokeEvent( UpdateDownloaded, new HelpUpdateDownloadedEventArgs( plugin, r ) );

            //    return r;
            //} );

            return null;
        }

        void AutoInstallUpdate( INamedVersionedUniqueId plugin, DownloadResult downloadResult )
        {
            if( IsInstallationPossibleAutomatically( plugin, downloadResult ) )
            {
                DoInstallUpdate( plugin, downloadResult );
            }
        }

        bool IsInstallationPossibleAutomatically( IVersionedUniqueId plugin, DownloadResult downloadResult )
        {
            Version manifestVersion = new Version( downloadResult.Version );

            Debug.Assert( plugin.UniqueId.ToString( "B" ) == downloadResult.Manifest.PluginId );
            Debug.Assert( plugin.Version >= manifestVersion );

            var currentHelpFile = new FileInfo( _helpContents.GetHelpContentFilePath( plugin, CultureInfo.CurrentCulture.TextInfo.CultureName ) );
            if( currentHelpFile.FullName != _helpContents.NoContentFilePath )
            {
                string currentCulture = currentHelpFile.Directory.Name;
                Version currentVersion = new Version( currentHelpFile.Directory.Parent.Name );

                // if the manifest version is lesser than the current version we shouldn't install the help content
                // otherwise we can delete a better local version
                if( currentVersion <= manifestVersion )
                {
                    return downloadResult.Culture == CultureInfo.CurrentCulture.TextInfo.CultureName
                        || downloadResult.Culture == CultureInfo.CurrentCulture.TwoLetterISOLanguageName
                        || downloadResult.Culture == currentCulture;
                }

                return false;
            }
            else
                return true; // by default if there is no local help, use the remote one it's better than nothing
        }

        void ManualInstallUpdate( INamedVersionedUniqueId plugin, IDownloadResult downloadResult )
        {
            DoInstallUpdate( plugin, downloadResult, true );
            InvokeEvent( UpdateInstalled, new HelpUpdateDownloadedEventArgs( plugin, downloadResult ) );
        }

        void DoInstallUpdate( INamedVersionedUniqueId plugin, IDownloadResult downloadResult, bool clean = false )
        {
            SimpleVersionedUniqueId pluginIdBasedOnManifest = new SimpleVersionedUniqueId( plugin.UniqueId, Version.Parse( downloadResult.Version ) );
            _helpContents.InstallDownloadedHelpContent( pluginIdBasedOnManifest, () => File.OpenRead( downloadResult.File.Path ), downloadResult.Culture, clean );
        }

        #endregion

        #region Manual Update

        public void StartManualUpdate()
        {
            var vm = new MainViewModel( this, ManualInstallUpdate );
            var wnd = new MainView();
            wnd.DataContext = vm;

            wnd.Show();

            vm.IsBusy = true;
            IList<INamedVersionedUniqueId> pluginsToProcess =  PluginRunner.PluginHost.LoadedPlugins.Cast<INamedVersionedUniqueId>().ToList();
            pluginsToProcess.Add( HostHelp.FakeHostHelpId );

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5 };

            Task.Factory
                .StartNew( () => Parallel.ForEach( pluginsToProcess, parallelOptions, item => ManualUpdateAsync( item ).Wait() ) )
                .ContinueWith( t => vm.IsBusy = false );
        }

        #endregion

    }
}
