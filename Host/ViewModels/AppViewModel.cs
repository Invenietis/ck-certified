#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\AppViewModel.cs) is part of CiviKey. 
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
using CK.Windows.App;

namespace Host
{
    public class AppViewModel : Conductor<IScreen>, IHostManipulator
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

            CivikeyHost.Context.ServiceContainer.Add<IHostManipulator>( this );
        }

        public void ToggleMinimize()
        {
            IsMinimized = !IsMinimized;
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
                if( !_isMinimized ) this.EnsureMainWindowVisible();
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

                ModalViewModel mvm = new ModalViewModel( R.Exit, R.ExitConfirmation );
                mvm.Buttons.Add( new ModalButton( mvm, R.Yes, null, ModalResult.Yes ) );
                mvm.Buttons.Add( new ModalButton( mvm, R.No, null, ModalResult.No ) );

                CustomMsgBox customMessageBox = new CustomMsgBox( ref mvm );
                customMessageBox.ShowDialog();
                e.Cancel = mvm.ModalResult != ModalResult.Yes;
                
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
                    _ensureMainWindowVisibleCommand = new CK.WPF.ViewModel.VMCommand( EnsureMainWindowVisible );
                return _ensureMainWindowVisibleCommand;
            }
        }

        void EnsureMainWindowVisible()
        {
            if( IsMinimized ) IsMinimized = false;
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
