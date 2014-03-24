using System;
using System.Windows;
using CK.Context;
using CK.Core;
using CK.Plugin;
using Help.Services;
using Host.Services;

namespace Help
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class HelpViewer : IPlugin, IHelpViewerService
    {
        const string PluginGuidString = "{1DB78D66-B5EC-43AC-828C-CCAB91FA6210}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.2";
        const string PluginPublicName = "Help viewer";
        public readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginIdVersion, PluginPublicName );

        HelpBrowser _helpBrowser;
        HelpContentManipulator _helpContents;

        [RequiredService]
        public IHostInformation HostInformations { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IHelpUpdaterService> HelpUpdater { get; set; }

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
            HostHelp.ShowHostHelp += OnHostRequestShowHelp;
        }

        public void Stop()
        {
            HostHelp.ShowHostHelp -= OnHostRequestShowHelp;
            if( _helpBrowser != null ) _helpBrowser.Close();
        }

        public void Teardown()
        {
        }
        
        public bool ShowHelpFor( IVersionedUniqueId pluginName, bool force = false )
        {
            string url = _helpContents.GetHelpContentFilePath( pluginName );
            bool found = url != _helpContents.NoContentFilePath;
            if( found || force )
            {
                Application.Current.Dispatcher.BeginInvoke( (Action)(() =>
                {
                    HelpBrowser._browser.Navigate( url );
                    HelpBrowser.Show();
                }), null );
            }
            return found;
        }

        HelpBrowser HelpBrowser
        {
            get
            {
                if( _helpBrowser == null )
                {
                    _helpBrowser = new HelpBrowser();
                    _helpBrowser.Closed += OnHelpBrowserClosed;
                }
                return _helpBrowser;
            }
        }

        void OnHelpBrowserClosed( object sender, EventArgs e )
        {
            _helpBrowser.Closed -= OnHelpBrowserClosed;
            _helpBrowser = null;
        }

        void OnHostRequestShowHelp( object sender, EventArgs e )
        {
            ShowHelpFor( HostHelp.FakeHostHelpId, true );
        }
    }
}
