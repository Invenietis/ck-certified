#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\SimpleSkin.cs) is part of CiviKey. 
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
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using Host.Services;
using SimpleSkin.ViewModels;
using CK.Windows;
using CK.Windows.Helpers;
using System.Linq;
using CommonServices.Accessibility;
using System.Diagnostics;
using CK.Plugins.SendInputDriver;
using System.IO;
using Help.Services;
using HighlightModel;

namespace SimpleSkin
{

    [Plugin( PluginIdString,
        PublicName = PluginPublicName,
        Version = PluginIdVersion,
        Categories = new[] { "Visual", "Accessibility" } )]
    public partial class MainKeyboardManager : IPlugin, IHaveDefaultHelp
    {
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );
        const string PluginIdString = "{36C4764A-111C-45e4-83D6-E38FC1DF5979}";
        readonly Guid PluginGuid = new Guid( PluginIdString );
        const string PluginPublicName = "MainKeyboardManager";
        const string PluginIdVersion = "1.6.0";

        public bool IsViewHidden { get { return _viewHidden; } }
        public IPluginConfigAccessor Config { get; set; }
        VMContextCurrentKeyboardSimple _ctxVm;
        IHostManipulator _hostManipulator;
        DispatcherTimer _timer;
        SkinWindow _skinWindow;
        MiniViewVM _miniViewVm;
        MiniView _miniView;
        bool _forceClose;
        bool _viewHidden;
        bool _isStarted;
        bool _autohide;
        int _timeout;

        //Since the IHostManipulator implementation is pushed to the servicecontainer after plugins are discovered and loaded, we cant use the RequiredService tag to fetch a ref to the HostManipulator.
        /// <summary>
        /// The HostManipulator, enables minimizing the host.
        /// </summary>
        public IHostManipulator HostManipulator { get { return _hostManipulator ?? ( _hostManipulator = Context.ServiceContainer.GetService<IHostManipulator>() ); } }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ISendStringService> SendStringService { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IHelpViewerService> HelpService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        [RequiredService]
        public INotificationService Notification { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        public IVersionedUniqueId PluginUniqueId
        {
            get { return PluginId; }
        }

        #region IPlugin Implementation

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            if( KeyboardContext.Status == InternalRunningStatus.Started && KeyboardContext.Service.Keyboards.Count > 0 )
            {
                //_ctxVm = new VMContextCurrentKeyboardSimple(
                //    Context,
                //    KeyboardContext.Service.Keyboards.Context,
                //    Config,
                //    _skinDispatcher );

                _isStarted = true;
                KeyboardContext.Service.CurrentKeyboard.IsActive = true;
                //_skinWindow = _noFocusWindowManager.CreateNoFocusWindow( () => new SkinWindow() { DataContext = _ctxVm } );

                //var defaultPlacement = new WINDOWPLACEMENT();

                //_skinDispatcher.Invoke( (System.Action)( () =>
                //{
                //    InitializeWindowLayout();
                //    _skinWindow.Show();
                //    defaultPlacement = CKWindowTools.GetPlacement( _skinWindow.Hwnd );
                //} ), null );

                //Sets on the Config must always be done on the main UI thread
                //WINDOWPLACEMENT actualPlacement = Config.User.GetOrSet( PlacementString, defaultPlacement );

                //Placing the skin at the same location as the last launch.
                //_skinDispatcher.Invoke( (Action)( () => CKWindowTools.SetPlacement( _skinWindow.Hwnd, actualPlacement ) ), null );

                //InitializeHighligther();
                //UpdateAutoHideConfig();

                RegisterEvents();

                OnSuccessfulStart();
            }
            else
            {
                _isStarted = false;
                Application.Current.Dispatcher.BeginInvoke( (Action)( () =>
                {
                    if( Notification != null )
                    {
                        Notification.ShowNotification( PluginId.UniqueId, "Aucun clavier n'est disponible",
                            "Aucun clavier n'est disponible dans le contexte actuel, veuillez choisir un contexte contenant au moins un clavier.", 1000, NotificationTypes.Error );
                    }
                } ), null );
            }
        }

        partial void OnSuccessfulStart();

        partial void OnSuccessfulStop();

        public void Stop()
        {
            if( _isStarted )
            {
                //UnInitializeHighlighter();

                Context.ServiceContainer.Remove( typeof( IPluginConfigAccessor ) );

                UnregisterEvents();
                _forceClose = true;

                //Setting the config is done after closing the window because modifying a value in the Config
                ///Triggers a Caliburn Micro OnNotifyPropertyChanged, which calls an Invoke on the main UI Thread.
                //generating random locks.
                //Once the LayoutManager is ready, we won't need this anymore.
                //WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                //_skinDispatcher.Invoke( (Action)(() =>
                //{
                //    placement = CKWindowTools.GetPlacement( _skinWindow.Hwnd );
                //    _skinWindow.Close();
                //}) );

                //Config.User.Set( PlacementString, placement );


                if( _miniViewVm != null )
                    _miniViewVm.Dispose();

                KeyboardContext.Service.CurrentKeyboard.IsActive = false;
                _isStarted = false;

                OnSuccessfulStop();
            }
        }

        public void Teardown()
        {
        }

        #region ToolMethods

        /// <summary>
        /// Must be called from the skin thread
        /// </summary>
        private void InitializeWindowLayout()
        {
            int defaultWidth = _ctxVm.KeyboardVM.W;
            int defaultHeight = _ctxVm.KeyboardVM.H;

            if( !Config.User.Contains( PlacementString ) )
            {
                var viewPortSize = Config[_ctxVm.KeyboardContext.CurrentKeyboard.CurrentLayout]["ViewPortSize"];
                if( viewPortSize != null )
                {
                    Size size = (Size)viewPortSize;
                    SetDefaultWindowPosition( (int)size.Width, (int)size.Height );
                }
                else
                    SetDefaultWindowPosition( defaultWidth, defaultHeight ); //first launch : places the skin in the default position
            }
            else _skinWindow.Width = _skinWindow.Height = 0; //After the first launch : hiding the window to get its last placement from the user conf.
        }

        /// <summary>
        /// Must be called from the skin thread
        /// </summary>
        /// <param name="defaultWidth"></param>
        /// <param name="defaultHeight"></param>
        private void SetDefaultWindowPosition( int defaultWidth, int defaultHeight )
        {
            _skinWindow.Top = 0;
            _skinWindow.Left = 0;
            _skinWindow.Width = defaultWidth;
            _skinWindow.Height = defaultHeight;
        }

        void UpdateAutoHideConfig()
        {
            var autohideCfg = Config.User["autohide"];
            var timeoutCfg = Config.User["autohide-timeout"];

            if( autohideCfg != null && Boolean.TryParse( autohideCfg.ToString(), out _autohide ) )
            {
                if( !_autohide && _timer != null ) _timer.Stop();
                if( timeoutCfg != null ) Int32.TryParse( timeoutCfg.ToString(), out _timeout );
            }
        }

        private string PlacementString
        {
            get
            {
                if( _ctxVm.KeyboardContext != null && _ctxVm.KeyboardContext.CurrentKeyboard != null )
                    return _ctxVm.KeyboardContext.CurrentKeyboard.Name + ".WindowPlacement";
                return "";
            }
        }

        #endregion

        #endregion

        #region OnXXXX

        private void RegisterEvents()
        {
            Config.ConfigChanged += new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );
            //_skinWindow.Closing += new CancelEventHandler( OnWindowClosing );
            //_skinWindow.MouseLeave += new System.Windows.Input.MouseEventHandler( OnMouseLeaveWindow );
            //_skinWindow.MouseEnter += new System.Windows.Input.MouseEventHandler( OnMouseEnterWindow );
            //_skinWindow.SizeChanged += new SizeChangedEventHandler( OnWindowResized );
            KeyboardContext.Service.CurrentKeyboardChanging += new EventHandler<CurrentKeyboardChangingEventArgs>( OnCurrentKeyboardChanging );
            KeyboardContext.Service.CurrentKeyboardChanged += new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
        }

        private void UnregisterEvents()
        {
            Config.ConfigChanged -= new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );
            KeyboardContext.Service.CurrentKeyboardChanging -= new EventHandler<CurrentKeyboardChangingEventArgs>( OnCurrentKeyboardChanging );
            KeyboardContext.Service.CurrentKeyboardChanged -= new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
            //_skinWindow.Closing -= new CancelEventHandler( OnWindowClosing );
            //_skinWindow.MouseLeave -= new System.Windows.Input.MouseEventHandler( OnMouseLeaveWindow );
            //_skinWindow.MouseEnter -= new System.Windows.Input.MouseEventHandler( OnMouseEnterWindow );
            //_skinWindow.SizeChanged -= new SizeChangedEventHandler( OnWindowResized );
        }

        void OnCurrentKeyboardChanging( object sender, CurrentKeyboardChangingEventArgs e )
        {
            if( e.Current != null ) e.Current.IsActive = false;

            //if( Highlighter.Status == InternalRunningStatus.Started )
            //{
            //    Highlighter.Service.UnregisterTree( "Keyboard", _ctxVm.KeyboardVM );
            //}

            //UnInitializeHighlighter();


            ////Saving the state of the window before doing anything (if the current keyboard is not null)
            //if( e.Current != null && _skinWindow != null )
            //{
            //    WINDOWPLACEMENT placement = new WINDOWPLACEMENT();

            //    //Invoke instead of beginInvoke because we need to save this configuration BEFORE the keyboard is changed.
            //    _skinDispatcher.Invoke( (Action)( () => placement = CKWindowTools.GetPlacement( _skinWindow.Hwnd ) ), null );
            //    Config.User.Set( PlacementString, placement );
            //}

            //if( e.Next == null )
            //{
            //    _skinDispatcher.BeginInvoke( (Action)( () =>
            //    {
            //        if( _miniView != null && _miniView.IsVisible )
            //        {
            //            Debug.Assert( !_viewHidden, "The miniview is visible yet _viewHidden is false" );
            //            _miniView.Hide();
            //        }

            //        if( _skinWindow != null && _skinWindow.IsVisible )
            //        {
            //            _skinWindow.Hide();
            //        }
            //    } ), null );
            //}
            //else
            //{
            //    //if the previous keyboard was null
            //    if( e.Current == null )
            //    {
            //        _skinDispatcher.BeginInvoke( (Action)( () =>
            //        {
            //            //if the view was not hidden before setting the keyboard to null
            //            if( _skinWindow != null && !_viewHidden )
            //            {
            //                Debug.Assert( !_skinWindow.IsVisible, "Changing the current keyboard from null to an existing keyboard, but the skin view was already visible" );
            //                _skinWindow.Show();
            //            }
            //            else if( _miniView != null )
            //            {
            //                Debug.Assert( !_miniView.IsVisible, "Changing the current keyboard from null to an existing keyboard, but the miniview was already visible" );
            //                _miniView.Show();
            //            }
            //        } ), null );
            //    }
            //}
        }

        void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            //InitializeHighligther();

            //if( Highlighter.Status == InternalRunningStatus.Started )
            //{
            //    Highlighter.Service.RegisterTree( "Keyboard", _ctxVm.KeyboardVM );
            //}

            //if( e.Current != null ) e.Current.IsActive = true;

            //if( e.Current != null && _skinWindow != null )
            //{
            //    if( Config.User[PlacementString] != null )
            //    {
            //        WINDOWPLACEMENT placement = (WINDOWPLACEMENT)Config.User[PlacementString];
            //        if( _viewHidden ) placement.showCmd = 0;
            //        else placement.showCmd = 8; //Show without taking focus

            //        _skinDispatcher.BeginInvoke( (Action)( () => CKWindowTools.SetPlacement( _skinWindow.Hwnd, placement ) ), null );
            //    }
            //    else
            //    {
            //        int w;
            //        int h;
            //        var viewPortSize = Config[_ctxVm.KeyboardContext.CurrentKeyboard.CurrentLayout]["ViewPortSize"];
            //        if( viewPortSize != null )
            //        {
            //            Size size = (Size)viewPortSize;
            //            w = (int)size.Width;
            //            h = (int)size.Height;
            //        }
            //        else
            //        {
            //            w = _ctxVm.KeyboardVM.W;
            //            h = _ctxVm.KeyboardVM.H;
            //        }

            //        _skinDispatcher.BeginInvoke( (Action)( () => SetDefaultWindowPosition( w, h ) ), null );
            //    }
            //}
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( ( c ) => c.UniqueId.Equals( this.PluginGuid ) ) && !String.IsNullOrEmpty( e.Key ) )
            {
                if( e.Key == "autohide" || e.Key == "autohide-timeout" ) UpdateAutoHideConfig();
            }
        }

        void OnMouseLeaveWindow( object sender, System.Windows.Input.MouseEventArgs e )
        {
            int timeout = _timeout == 0 ? 6000 : _timeout;
            //if( _autohide )
            //{
            //    if( _timer == null )
            //        _timer = new DispatcherTimer( TimeSpan.FromMilliseconds( timeout ), DispatcherPriority.Normal, ( o, args ) => HideSkin(), _skinWindow.Dispatcher );
            //    else
            //        _timer.Interval = TimeSpan.FromMilliseconds( timeout );

            //    _timer.Start();
            //}
        }

        void OnMouseEnterWindow( object sender, System.Windows.Input.MouseEventArgs e )
        {
            if( _timer != null ) _timer.Stop();
        }

        void OnWindowResized( object sender, SizeChangedEventArgs e )
        {
            if( _timer != null ) _timer.Stop();
        }

        void OnWindowClosing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            if( !_forceClose && !e.Cancel )
            {
                e.Cancel = true;
                if( Notification != null )
                {
                    Notification.ShowNotification( new Guid( PluginIdString ), "Unable to close skin window",
                        "The skin window cannot be closed like this, if you want to close the window stop the plugin.", 5000, NotificationTypes.Warning );
                }
            }
        }

        #endregion

        #region Hightlight Methods

        void OnHighlighterServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                RegisterHighlighter();
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                UnregisterHighlighter();
            }
        }

        private void InitializeHighligther()
        {
            Highlighter.ServiceStatusChanged += OnHighlighterServiceStatusChanged;
            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                RegisterHighlighter();
            }
        }

        private void UnInitializeHighlighter()
        {
            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                UnregisterHighlighter();
            }
            Highlighter.ServiceStatusChanged -= OnHighlighterServiceStatusChanged;
        }

        private void UnregisterHighlighter()
        {
            //Highlighter.Service.UnregisterTree( "Keyboard", _ctxVm.KeyboardVM );
        }

        private void RegisterHighlighter()
        {
            //Highlighter.Service.RegisterTree( "Keyboard", new ExtensibleHighlightableElementProxy( _ctxVm.KeyboardVM.Keyboard.Name, _ctxVm.KeyboardVM ) );
        }

        #endregion

        #region MiniView methods & properties

        /// <summary>
        /// Gets whether the application's host's window is minimized.
        /// </summary>
        public bool IsHostMinimized { get { return HostManipulator.IsMinimized; } }

        /// <summary>
        /// Hides the keyboard's MiniView and shows the keyboard
        /// </summary>
        public void RestoreSkin()
        {
            //if( _viewHidden )
            //{
            //    _viewHidden = false;

            //    _skinDispatcher.BeginInvoke( (Action)( () =>
            //    {
            //        _miniView.Hide();
            //        _skinWindow.Show();
            //    } ), null );

            //    if( Highlighter.Status == InternalRunningStatus.Started )
            //    {
            //        Highlighter.Service.RegisterTree( "Keyboard", _ctxVm.KeyboardVM );
            //        Highlighter.Service.UnregisterTree( "MinimizeKeyboard", _miniViewVm );
            //    }
            //}
        }

        //void ShowMiniView()
        //{
        //    if( _miniView == null )
        //    {
        //        _miniViewVm = new MiniViewVM( this );

        //        _miniView = new MiniView( RestoreSkin ) { DataContext = _miniViewVm };
        //        _miniView.Closing += OnWindowClosing;
        //        _miniView.Show();

        //        if( !ScreenHelper.IsInScreen( new System.Drawing.Point( _miniViewVm.X + (int)_miniView.ActualWidth / 2, _miniViewVm.Y + (int)_miniView.ActualHeight / 2 ) ) ||
        //        !ScreenHelper.IsInScreen( new System.Drawing.Point( _miniViewVm.X + (int)_miniView.ActualWidth, _miniViewVm.Y + (int)_miniView.ActualHeight ) ) )
        //        {
        //            _miniView.Left = 0;
        //            _miniView.Top = 0;
        //        }
        //    }
        //    else
        //    {
        //        _miniView.Show();
        //    }
        //}

        #endregion

        #region IHaveDefaultHelp Members

        public Stream GetDefaultHelp()
        {
            return typeof( MainKeyboardManager ).Assembly.GetManifestResourceStream( "SimpleSkin.Res.helpcontent.zip" );
        }

        #endregion
    }


}
