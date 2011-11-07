using System;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using CK.Core;
using CK.Keyboard.Model;
using CK.WPF.Controls;
using CK.WPF.ViewModel;
using Host.Resources;
using Host.Services;
using Host.VM;
using CK.Plugin;
using CK.Plugin.Config;
using System.Diagnostics;

namespace Host
{
    public class AppViewModel : Conductor<IScreen>
    {
        bool _closing;

        public AppViewModel()
        {
            DisplayName = R.CiviKey;
            EnsureMainWindowVisibleCommand = new VMCommand( EnsureMainWindowVisible, null );
            ConfigManager = new ConfigManager();
            ConfigManager.ActivateItem( new RootConfigViewModel( this ) );
            ActivateItem( ConfigManager );

            CivikeyHost.Context.OnExitApplication += ( o, e ) => ExitHost( e.HostShouldExit );
            CivikeyHost.Context.BeforeExitApplication += new EventHandler<CK.Context.ApplicationExitingEventArgs>( OnBeforeExitApplication );
        }

        public CivikeyStandardHost CivikeyHost { get { return CivikeyStandardHost.Instance; } }

        public IKeyboardContext KeyboardContext { get { return CivikeyHost.Context.GetService<IKeyboardContext>(); } }

        public INotificationService NotificationCtx { get { return CivikeyHost.Context.GetService<INotificationService>(); } }

        internal IConfigContainer ConfigContainer { get { return CivikeyHost.Context.GetService<IConfigContainer>(); } }

        internal ISimplePluginRunner PluginRunner { get { return CivikeyHost.Context.PluginRunner; } }

        public ConfigManager ConfigManager { get; private set; }

        public string AppVersion { get { return CivikeyHost.Version.ToString(); } }

        public bool ShowTaskbarIcon
        {
            get { return CivikeyHost.UserConfig.GetOrSet( "ShowTaskbarIcon", true ); }
            set
            {
                if( CivikeyHost.UserConfig.Set( "ShowTaskbarIcon", value ) != ChangeStatus.None )
                {
                    if( !value && !ShowSystrayIcon ) ShowSystrayIcon = true;
                    NotifyOfPropertyChange( "ShowTaskbarIcon" );
                }
            }
        }

        public bool ShowSystrayIcon
        {
            get { return CivikeyStandardHost.Instance.UserConfig.GetOrSet( "ShowSystrayIcon", true ); }
            set
            {
                if( CivikeyStandardHost.Instance.UserConfig.Set( "ShowSystrayIcon", value ) != ChangeStatus.None )
                {
                    if( !value && !ShowTaskbarIcon ) ShowTaskbarIcon = true;
                    NotifyOfPropertyChange( "ShowSystrayIcon" );
                }
            }
        }

        protected override void OnViewLoaded( object view )
        {
            // Ensures that the view is actually the main application window:
            // since the application is configured with ShutdownMode="OnMainWindowClose", 
            // closing the window exits the application.
            App.Current.MainWindow = (Window)view;
            base.OnViewLoaded( view );
            CivikeyHost.Initialize();
        }

        bool _forceClose;

        public override void CanClose( Action<bool> callback )
        {
            callback( _forceClose || CivikeyHost.Context.RaiseExitApplication( false ) );
        }

        void OnBeforeExitApplication( object sender, CK.Context.ApplicationExitingEventArgs e )
        {
            if( !_closing )
            {
                _closing = true;
                Window thisView = GetView( null ) as Window;
                Window bestParent = App.Current.GetTopWindow();
                e.Cancel = System.Windows.MessageBox.Show( bestParent, R.ConfirmExitApp, R.Exit, MessageBoxButton.YesNo, MessageBoxImage.Question ) != MessageBoxResult.Yes;
                if( bestParent != thisView ) thisView.Activate();
                _closing = !e.Cancel;
            }
            else
                e.Cancel = true;
        }

        void ExitHost( bool hostShouldExit )
        {
            if( hostShouldExit )
            {
                var thisView = GetView( null ) as Window;
                if( thisView != null ) thisView.Hide();
            }

            CivikeyHost.SaveContext();
            CivikeyHost.SaveUserConfig();
            CivikeyHost.SaveSystemConfig();

            if( hostShouldExit )
            {
                _forceClose = true;
                this.TryClose();
            }
        }

        public ICommand EnsureMainWindowVisibleCommand { get; private set; }

        void EnsureMainWindowVisible()
        {
            App.Current.MainWindow.WindowState = WindowState.Normal;
            App.Current.MainWindow.Activate();
        }

        internal void StartPlugin( Guid pluginId )
        {
            var runner = CivikeyHost.Context.GetService<ISimplePluginRunner>( true );
            CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( pluginId, CK.Plugin.Config.ConfigUserAction.Started );
            runner.Apply();
        }

        internal void StopPlugin( Guid pluginId )
        {
            var runner = CivikeyHost.Context.GetService<ISimplePluginRunner>( true );
            CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( pluginId, CK.Plugin.Config.ConfigUserAction.Stopped );
            runner.Apply();
        }

        internal bool IsPluginRunning( IPluginInfo plugin )
        {
            var runner = CivikeyHost.Context.GetService<ISimplePluginRunner>( true );
            return runner.PluginHost.IsPluginRunning( plugin );
        }

        internal IDisposable ShowBusyIndicator()
        {
            return new WaitHandle( GetView( null ) as Window );
        }
    }

    public class WaitHandle : IDisposable
    {
        Window _owner;
        Cursor _previousCursor;

        public WaitHandle( Window owner )
        {
            Debug.Assert( owner != null );
            _owner = owner;
            _previousCursor = _owner.Cursor;

            _owner.Cursor = Cursors.Wait;
        }

        public void Dispose()
        {
            _owner.Cursor = _previousCursor;
        }
    }
}
