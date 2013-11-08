using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            return true;
        }

        public void Start()
        {
            _helpContents = new HelpContentManipulator( HostInformations );
            PluginRunner.ApplyDone += OnPluginRunnerApplyDone;

            _helpContents.FindOrCreateBaseContent();
            RegisterAllAvailableDefaultHelpContents( includeHost: true );
        }


        public void Stop()
        {
            PluginRunner.ApplyDone -= OnPluginRunnerApplyDone;
        }

        public void Teardown()
        {
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

        public void SoftUpdate()
        {

        }

        public void FullUpdate()
        {

        }

        #endregion
    }
}
