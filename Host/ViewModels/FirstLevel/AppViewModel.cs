#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\FirstLevel\AppViewModel.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
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
using Host.Resources;
using CK.Windows.App;
using System.Windows.Interop;
using System.Windows.Threading;
using Help.Services;
using CK.Windows;

namespace Host
{
    public class AppViewModel : Conductor<IScreen>, IHostManipulator
    {
        bool _forceClose;
        bool _closing;
        bool _isVisible;

        public AppViewModel()
        {
            DisplayName = "CiviKey";
            ConfigManager = new ConfigManager();
            ConfigManager.ActivateItem( new RootConfigViewModel( this ) );
            ActivateItem( ConfigManager );

            IsVisible = true;
            IsMinimized = true;

            CivikeyHost.Context.ApplicationExited += ( o, e ) => ExitHost( e.HostShouldExit );
            CivikeyHost.Context.ApplicationExiting += new EventHandler<CK.Context.ApplicationExitingEventArgs>( OnBeforeExitApplication );

            CivikeyHost.Context.ServiceContainer.Add<IHostManipulator>( this );
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

        public IHelpUpdaterService HelpUpdaterService { get { return CivikeyHost.Context.GetService<IHelpUpdaterService>(); } }

        public string AppVersion { get { return CivikeyHost.AppVersion.ToString(); } }

        /// <summary>
        /// Gets whether the application should be visible in the TaskBar.
        /// </summary>
        public bool ShowTaskbarIcon
        {
            get { return CivikeyHost.UserConfig.GetOrSet( "ShowTaskbarIcon", true ); }
            set
            {
                Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );

                if( CivikeyHost.UserConfig.Set( "ShowTaskbarIcon", value ) != ChangeStatus.None )
                {
                    // v2.7.0 : the notificationmanager has been removed, so we don't have a systray icon anymore. Put this back on together with the notification manager.
                    //if( !value && !ShowSystrayIcon ) ShowSystrayIcon = true;
                    NotifyOfPropertyChange( "ShowTaskbarIcon" );
                }
            }
        }

        /// <summary>
        /// v2.7.0 : the notificationmanager has been removed, so we don't have a systray icon anymore. Put this back on together with the notification manager.
        /// Gets whether the application should be visible in the Systray.
        /// </summary>
        //public bool ShowSystrayIcon
        //{
        //    get
        //    {
        //        return CivikeyStandardHost.Instance.UserConfig.GetOrSet( "ShowSystrayIcon", true );
        //    }
        //    set
        //    {
        //        if( CivikeyStandardHost.Instance.UserConfig.Set( "ShowSystrayIcon", value ) != ChangeStatus.None )
        //        {
        //            if( !value && !ShowTaskbarIcon ) ShowTaskbarIcon = true;
        //            NotifyOfPropertyChange( "ShowSystrayIcon" );
        //        }
        //    }
        //}

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
            var thisView = GetView( null ) as Window;
            if( hostShouldExit )
            {
                if( thisView != null )
                {
                    thisView.Hide();
                }
            }

            CivikeyHost.SaveContext();
            CivikeyHost.SaveUserConfig();
            CivikeyHost.SaveSystemConfig();

            NoFocusManager.Default.Shutdown();

            if( hostShouldExit )
            {
                _forceClose = true;
                TryClose();
            }
        }

        /// <summary>
        /// Gets whether another window overlays the window set as parameter
        /// </summary>
        /// <returns></returns>
        public bool IsOverlayed()
        {
            var view = GetView( null ) as Window;
            return CK.Windows.Helpers.WindowHelper.IsOverLayed( view ); ;
        }

        void OnBeforeExitApplication( object sender, CK.Context.ApplicationExitingEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );

            if( !_closing )
            {
                if( !e.HostShouldExit )
                {
                    _closing = true;
                    var thisView = GetView( null ) as Window;
                    var bestParent = App.Current.GetTopWindow();

                    var mvm = new ModalViewModel( R.Exit, R.ExitConfirmation );
                    mvm.Buttons.Add( new ModalButton( mvm, R.Yes, null, ModalResult.Yes ) );
                    mvm.Buttons.Add( new ModalButton( mvm, R.No, null, ModalResult.No ) );
                    var customMessageBox = new CustomMsgBox( ref mvm );
                    customMessageBox.ShowDialog();

                    e.Cancel = mvm.ModalResult != ModalResult.Yes;

                    _closing = !e.Cancel;
                    if( !_closing && bestParent != thisView )
                    {
                        thisView.Activate();
                    }
                }
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

        ICommand _showHelpCommand;
        public ICommand ShowHelpCommand
        {
            get
            {
                return _showHelpCommand ?? (_showHelpCommand = new VMCommand( () =>
                {
                    CivikeyHost.FireShowHostHelp();
                } ));
            }
        }

        /// <summary>
        /// Gets whether the window is visible or not.
        /// This boolean is only valid when <see cref="Host.AppViewModel.ShowTaskbarIcon"/> is set to false, otherwise it doesn't track the actual visibility of the window.
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                NotifyOfPropertyChange( "IsVisible" );
            }
        }

        void EnsureMainWindowVisible()
        {
            if( IsMinimized ) IsMinimized = false;
            App.Current.MainWindow.Activate();
        }

        /// <summary>
        /// Toggle the Minimized state of the MainApplicationWindow.
        /// If the window should not appear in the Taskbar, it is hidden instead of minimized.
        /// This method must be called from the Application's Main Thread
        /// </summary>
        /// <param name="lastFocusedWindowsHandle"></param>
        public void ToggleMinimize( IntPtr lastFocusedWindowsHandle )
        {
            IntPtr hostWindowHandle = new WindowInteropHelper( (Window)GetView( null ) ).Handle;

            if( !IsMinimized && hostWindowHandle != lastFocusedWindowsHandle )
            {
                //If the window is not minimized but doesn't have the focus, it is activated.
                App.Current.MainWindow.Activate();
            }
            else
            {
                if( !ShowTaskbarIcon ) IsVisible = !IsVisible;
                IsMinimized = !IsMinimized;
            }
        }

        bool _isMinimized;
        public bool IsMinimized
        {
            get { return _isMinimized; }
            set
            {
                Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );

                if( !ShowTaskbarIcon ) IsVisible = !value;

                //We have to push this delegate on the dispatcher queue to have the different changes processed correctly
                Application.Current.Dispatcher.BeginInvoke( DispatcherPriority.Background,
                    new System.Action( () =>
                    {
                        _isMinimized = value;
                        NotifyOfPropertyChange( "IsMinimized" );
                        App.Current.MainWindow.Activate();
                    } )
                );
            }
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
