using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WindowManager.Model;
using CK.Windows;
using CK.Windows.Helpers;
using CommonServices.Accessibility;
using SimpleSkin.Res;

namespace SimpleSkin
{
    [Plugin( WindowStateManager.PluginIdString, PublicName = PluginPublicName, Version = WindowStateManager.PluginIdVersion,
       Categories = new string[] { "Visual", "Accessibility" } )]
    public class WindowStateManager : IPlugin
    {
        const string PluginPublicName = "WindowStateManager";
        const string PluginIdString = "{3F8140F5-AD63-4EF4-AB6C-A9A7EE18078A}";
        const string PluginIdVersion = "1.0.0";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWindowManager WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        bool _viewHidden;
        MiniViewVM _miniViewVm;
        MiniView _miniView;

        public bool IsViewHidden { get { return _viewHidden; } }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _miniViewVm = new MiniViewVM( this );
            RegisterEvents();
        }

        public void Stop()
        {
            UnregisterEvents();
            UninitializeMiniview();
        }

        public void Teardown()
        {
        }

        #endregion

        private void RegisterEvents()
        {
            RegisterWindowEvents();

            Highlighter.ServiceStatusChanged += OnHighlighterStatusChanged;

            KeyboardContext.ServiceStatusChanged += OnKeyboardContextStatusChanged;
            RegisterKeyboardEvents();
        }

        private void UnregisterEvents()
        {
            UnregisterWindowEvents();

            Highlighter.ServiceStatusChanged -= OnHighlighterStatusChanged;

            UnregisterKeyboardEvents();
            KeyboardContext.ServiceStatusChanged -= OnKeyboardContextStatusChanged;
        }

        private void UninitializeMiniview()
        {
            if( _miniView != null )
            {
                UnregisterFromHighlighter();

                _miniView.Close();
                _viewHidden = false;
            }

        }

        #region Highlighter Members

        void OnHighlighterStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                RegisterFromHighlighter();
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                UnregisterFromHighlighter();
            }
        }

        private void RegisterFromHighlighter()
        {
            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                Highlighter.Service.RegisterTree( _miniViewVm.Name, R.MiniViewName, _miniViewVm );
                Highlighter.Service.HighlightImmediately( _miniViewVm );
            }
        }

        private void UnregisterFromHighlighter()
        {
            if( Highlighter.Status == InternalRunningStatus.Stopping )
            {
                Highlighter.Service.UnregisterTree( _miniViewVm.Name, _miniViewVm );
            }
        }

        #endregion Highlighter Members

        #region WindowManager Members

        private void RegisterWindowEvents()
        {
            Debug.Assert( WindowManager != null );

            WindowManager.Unregistered += OnWindowManagerUnregistered;
            WindowManager.WindowMinimized += OnWindowMinimized;
            WindowManager.WindowRestored += OnWindowRestored;
        }

        void OnWindowManagerUnregistered( object sender, WindowElementEventArgs e )
        {
            if( WindowManager.WindowElements.Count == 1 && _miniView != null && _miniView.Visibility != Visibility.Hidden )
            {
                UnregisterFromHighlighter();

                _miniView.Hide();
                _viewHidden = false;
            }
        }

        private void UnregisterWindowEvents()
        {
            WindowManager.WindowMinimized += OnWindowMinimized;
            WindowManager.WindowRestored += OnWindowRestored;
        }

        void OnWindowRestored( object sender, WindowElementEventArgs e )
        {
            Debug.Assert( _viewHidden );

            RestoreWindows();
        }

        void OnWindowMinimized( object sender, WindowElementEventArgs e )
        {
            Debug.Assert( !_viewHidden );

            MinimizeWindows();
        }

        #endregion WindowManager Members

        #region KeyboardContext Members

        private void RegisterKeyboardEvents()
        {
            if( KeyboardContext.Status.IsStartingOrStarted )
            {
                KeyboardContext.Service.Keyboards.KeyboardActivated += OnKeyboardEvent;
                KeyboardContext.Service.Keyboards.KeyboardDeactivated += OnKeyboardEvent;
                KeyboardContext.Service.Keyboards.KeyboardDestroyed += OnKeyboardEvent;
                KeyboardContext.Service.Keyboards.KeyboardCreated += OnKeyboardEvent;
                KeyboardContext.Service.Keyboards.KeyboardRenamed += OnKeyboardEvent;
            }
        }

        private void UnregisterKeyboardEvents()
        {
            if( KeyboardContext.Status.IsStartingOrStarted )
            {
                KeyboardContext.Service.Keyboards.KeyboardDeactivated -= OnKeyboardEvent;
                KeyboardContext.Service.Keyboards.KeyboardActivated -= OnKeyboardEvent;
            }
        }

        private void OnKeyboardContextStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                RegisterKeyboardEvents();
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                UnregisterKeyboardEvents();
            }
        }

        private void OnKeyboardEvent( object sender, EventArgs e )
        {
            if( _viewHidden )
            {
                RestoreWindows();
            }
        }

        #endregion Keyboard

        public void RestoreWindows()
        {
            if( _miniView != null && _miniView.Visibility != Visibility.Hidden )
            {
                UnregisterFromHighlighter();
                _miniView.Hide();
            }

            //Dispatched afterwards
            WindowManager.RestoreAllWindows();

            _viewHidden = false;
        }

        public void MinimizeWindows()
        {
            if( !_viewHidden )
            {
                _viewHidden = true;

                ShowMiniView();

                WindowManager.MinimizeAllWindows();
            }
        }

        private void ShowMiniView()
        {
            if( _miniView == null )
            {

                _miniView = new MiniView( RestoreWindows ) { DataContext = _miniViewVm };
                _miniView.Show();

                if( !ScreenHelper.IsInScreen( new System.Drawing.Point( _miniViewVm.X + (int)_miniView.ActualWidth / 2, _miniViewVm.Y + (int)_miniView.ActualHeight / 2 ) ) ||
                !ScreenHelper.IsInScreen( new System.Drawing.Point( _miniViewVm.X + (int)_miniView.ActualWidth, _miniViewVm.Y + (int)_miniView.ActualHeight ) ) )
                {
                    _miniView.Left = 0;
                    _miniView.Top = 0;
                }
            }
            else
            {
                _miniView.Show();
            }

            RegisterFromHighlighter();
        }
    }
}
