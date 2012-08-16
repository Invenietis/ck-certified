using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Windows;
using CK.Windows.Config;
using CK.Keyboard.Model;
using Host.Services;
using CK.Plugin.Config;
using CK.Plugin;
using CK.Core;
using System.Windows.Input;
using System.Diagnostics;
using CK.WPF.ViewModel;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Host.Resources;

namespace Host
{
    public class AppViewModel : Conductor<IScreen>
    {
        bool _forceClose;
        bool _closing;

        public AppViewModel()
        {

            DisplayName = "CiviKey";//R.CiviKey
            ConfigManager = new ConfigManager();
            ConfigManager.ActivateItem( new RootConfigViewModel( this ) );
            ActivateItem( ConfigManager );

            IsMinimized = true;

            CivikeyHost.Context.ApplicationExited += ( o, e ) => ExitHost( e.HostShouldExit );
            CivikeyHost.Context.ApplicationExiting += new EventHandler<CK.Context.ApplicationExitingEventArgs>( OnBeforeExitApplication );

        }

        public CivikeyStandardHost CivikeyHost { get { return CivikeyStandardHost.Instance; } }

        IKeyboardContext _keyboardContext;
        public IKeyboardContext KeyboardContext
        {
            get
            {
                if( _keyboardContext == null )
                    _keyboardContext = CivikeyHost.Context.GetService<IKeyboardContext>();
                return _keyboardContext;
            }
        }

        public INotificationService NotificationCtx { get { return CivikeyHost.Context.GetService<INotificationService>(); } }

        internal IConfigContainer ConfigContainer { get { return CivikeyHost.Context.GetService<IConfigContainer>(); } }

        internal ISimplePluginRunner PluginRunner { get { return CivikeyHost.Context.PluginRunner; } }

        public ConfigManager ConfigManager { get; private set; }

        public string AppVersion { get { return CivikeyHost.AppVersion.ToString(); } }

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

        bool _isMinimized;
        public bool IsMinimized 
        {
            get { return _isMinimized; }
            set 
            { 
                _isMinimized = value; 
                NotifyOfPropertyChange( "IsMinimized" ); 
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
        }

        public override void CanClose( Action<bool> callback )
        {
            callback( _forceClose || CivikeyHost.Context.RaiseExitApplication( false ) );
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

        void OnBeforeExitApplication( object sender, CK.Context.ApplicationExitingEventArgs e )
        {
            if( !_closing )
            {
                _closing = true;
                Window thisView = GetView( null ) as Window;
                Window bestParent = App.Current.GetTopWindow();
                e.Cancel = System.Windows.MessageBox.Show( bestParent, R.ExitConfirmation, R.Exit, MessageBoxButton.YesNo, MessageBoxImage.Question ) != MessageBoxResult.Yes;
                if( bestParent != thisView ) thisView.Activate();
                _closing = !e.Cancel;
            }
            else
                e.Cancel = true;
        }

        ICommand _ensureMainWindowVisibleCommand;
        public ICommand EnsureMainWindowVisibleCommand
        {
            get
            {
                if( _ensureMainWindowVisibleCommand == null )
                    _ensureMainWindowVisibleCommand = new VMCommand( EnsureMainWindowVisible );
                return _ensureMainWindowVisibleCommand;
            }
        }

        void EnsureMainWindowVisible()
        {
            //App.Current.MainWindow.Show();
            IsMinimized = false;
            //App.Current.MainWindow.Visibility = Visibility.Visible;
            //App.Current.MainWindow.WindowState = WindowState.Normal;
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
