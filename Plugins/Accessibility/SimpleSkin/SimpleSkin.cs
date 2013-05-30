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
using CommonServices;
using Host.Services;
using SimpleSkin.ViewModels;
using CK.Windows;
using CK.Windows.Helpers;
using System.Linq;
using CommonServices.Accessibility;
using System.Diagnostics;
using CK.Plugins.SendInput;
using HighlightModel;
using System.Collections.Generic;
using CK.WPF.ViewModel;
using System.Threading;

namespace SimpleSkin
{
    public class WPFThread
    {
        public readonly Dispatcher Dispatcher;
        readonly object _lock;

        public WPFThread( string name )
        {
            _lock = new object();
            Thread t = new Thread( StartDispatcher );
            t.Name = name;
            t.SetApartmentState( ApartmentState.STA );
            lock( _lock )
            {
                t.Start();
                Monitor.Wait( _lock );
            }
            Dispatcher = Dispatcher.FromThread( t );
        }

        void StartDispatcher()
        {
            // This creates the Dispatcher and pushes the job.
            Dispatcher.CurrentDispatcher.BeginInvoke( (System.Action)DispatcherStarted, null );
            // Initializes a SynchronizationContext (for tasks ot other components that would require one). 
            SynchronizationContext.SetSynchronizationContext( new DispatcherSynchronizationContext( Dispatcher.CurrentDispatcher ) );
            Dispatcher.Run();
        }

        void DispatcherStarted()
        {
            lock( _lock )
            {
                Monitor.Pulse( _lock );
            }
        }
    }

    [Plugin( SimpleSkin.PluginIdString,
        PublicName = PluginPublicName,
        Version = SimpleSkin.PluginIdVersion,
        Categories = new string[] { "Visual", "Accessibility" } )]
    public class SimpleSkin : IPlugin, ISkinService
    {
        const string PluginIdString = "{36C4764A-111C-45e4-83D6-E38FC1DF5979}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "SimpleSkin";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        public bool IsViewHidden { get { return _viewHidden; } }
        public IPluginConfigAccessor Config { get; set; }
        IHostManipulator _hostManipulator;
        VMContextSimple _ctxVm;
        SkinWindow _skinWindow;
        DispatcherTimer _timer;
        MiniViewVM _miniViewVm;
        MiniView _miniView;
        bool _forceClose;
        bool _viewHidden;
        bool _isStarted;
        bool _autohide;
        int _timeout;

        //Since the IHotsManipulator implementation is pushed to the servicecontainer after plugins are discovered and loaded, we cant use the RequiredService tag to fetch a ref to the HostManipulator.
        /// <summary>
        /// The HostManipulator, enables minimizing the host.
        /// </summary>
        public IHostManipulator HostManipulator { get { return _hostManipulator ?? ( _hostManipulator = Context.ServiceContainer.GetService<IHostManipulator>() ); } }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ISendStringService> SendStringService { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IHelpService> HelpService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        [RequiredService]
        public INotificationService Notification { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        WPFThread _secondThread;

        #region IPlugin Implementation

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            if( HelpService.Status == InternalRunningStatus.Started ) HelpService.Service.RegisterHelpContent( PluginId, typeof( SimpleSkin ).Assembly.GetManifestResourceStream( "SimpleSkin.Res.helpcontent.zip" ) );

            if( KeyboardContext.Status == InternalRunningStatus.Started && KeyboardContext.Service.Keyboards.Count > 0 )
            {
                _secondThread = new WPFThread( "SkinThread" );
                _ctxVm = new VMContextSimple( Context, KeyboardContext.Service.Keyboards.Context, Config, _secondThread );

                _secondThread.Dispatcher.Invoke( (System.Action)( () =>
                {
                    _isStarted = true;
                    _skinWindow = new SkinWindow() { DataContext = _ctxVm };

                    //while( true ) { }

                    InitializeWindowLayout();


                    _skinWindow.Show();

                    //Placing the skin at the same location as the last launch.
                    _skinWindow.SetPlacement( (WINDOWPLACEMENT)Config.User.GetOrSet<WINDOWPLACEMENT>( PlacementString, _skinWindow.GetPlacement() ) );
                } ), null );
                InitializeHighligther();
                UpdateAutoHideConfig();

                RegisterEvents();
            }
            else
            {
                _isStarted = false;
                Notification.ShowNotification( PluginId.UniqueId, "Aucun clavier n'est disponible",
                    "Aucun clavier n'est disponible dans le contexte actuel, veuillez choisir un contexte contenant au moins un clavier.", 1000, NotificationTypes.Error );
            }
        }

        public void Stop()
        {
            if( _isStarted )
            {
                UnInitializeHighlighter();

                Context.ServiceContainer.Remove( typeof( IPluginConfigAccessor ) );

                UnregisterEvents();

                _secondThread.Dispatcher.BeginInvoke( (Action)( () => Config.User.Set( PlacementString, _skinWindow.GetPlacement() ) ) );

                _forceClose = true;
                _secondThread.Dispatcher.BeginInvoke( (Action)( () => _skinWindow.Close() ) );

                if( _miniView != null )
                {
                    _miniView.Close();
                    _miniView = null;
                    _viewHidden = false;
                }
                if( _miniView != null )
                    _miniViewVm.Dispose();

                _ctxVm.Dispose();
                _ctxVm = null;
                _isStarted = false;
            }
        }

        public void Teardown()
        {
        }

        #region ToolMethods

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
            _skinWindow.Closing += new CancelEventHandler( OnWindowClosing );
            _skinWindow.MouseLeave += new System.Windows.Input.MouseEventHandler( OnMouseLeaveWindow );
            _skinWindow.MouseEnter += new System.Windows.Input.MouseEventHandler( OnMouseEnterWindow );
            _skinWindow.SizeChanged += new SizeChangedEventHandler( OnWindowResized );
            _ctxVm.KeyboardContext.CurrentKeyboardChanging += new EventHandler<CurrentKeyboardChangingEventArgs>( OnCurrentKeyboardChanging );
            _ctxVm.KeyboardContext.CurrentKeyboardChanged += new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
        }

        private void UnregisterEvents()
        {
            Config.ConfigChanged -= new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );
            _ctxVm.KeyboardContext.CurrentKeyboardChanging -= new EventHandler<CurrentKeyboardChangingEventArgs>( OnCurrentKeyboardChanging );
            _ctxVm.KeyboardContext.CurrentKeyboardChanged -= new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
            _skinWindow.Closing -= new CancelEventHandler( OnWindowClosing );
            _skinWindow.MouseLeave -= new System.Windows.Input.MouseEventHandler( OnMouseLeaveWindow );
            _skinWindow.MouseEnter -= new System.Windows.Input.MouseEventHandler( OnMouseEnterWindow );
            _skinWindow.SizeChanged -= new SizeChangedEventHandler( OnWindowResized );
        }

        void OnCurrentKeyboardChanging( object sender, CurrentKeyboardChangingEventArgs e )
        {
            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                Highlighter.Service.UnregisterTree( _ctxVm.KeyboardVM );
            }

            //Saving the state of the window before doing anything (if the current keyboard is not null)
            if( e.Current != null && _skinWindow != null )
            {
                _ctxVm.Thread.Dispatcher.BeginInvoke( (Action)( () => Config.User.Set( PlacementString, _skinWindow.GetPlacement() ) ), null );
                //Config.User.Set( PlacementString, _skinWindow.GetPlacement() );
            }

            if( e.Next == null )
            {
                if( _miniView != null && _miniView.IsVisible )
                {
                    Debug.Assert( !_viewHidden, "The miniview is visible yet _viewHidden is false" );
                    _miniView.Hide();
                }

                if( _skinWindow != null && _skinWindow.IsVisible )
                {
                    _skinWindow.Hide();
                }
            }
            else
            {
                //if the previous keyboard was null
                if( e.Current == null )
                {
                    //if the view was not hidden before setting the keyboard to null
                    if( _skinWindow != null && !_viewHidden )
                    {
                        Debug.Assert( !_skinWindow.IsVisible, "Changing the current keyboard from null to an existing keyboard, but the skin view was already visible" );
                        _skinWindow.Show();
                    }
                    else if( _miniView != null )
                    {
                        Debug.Assert( !_miniView.IsVisible, "Changing the current keyboard from null to an existing keyboard, but the miniview was already visible" );
                        _miniView.Show();
                    }
                }
            }
        }

        void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                Highlighter.Service.RegisterTree( _ctxVm.KeyboardVM );
            }

            if( e.Current != null && _skinWindow != null )
            {
                if( Config.User[PlacementString] != null )
                {
                    WINDOWPLACEMENT placement = (WINDOWPLACEMENT)Config.User[PlacementString];
                    if( _viewHidden ) placement.showCmd = 0;
                    else placement.showCmd = 8; //Show without taking focus
                    _skinWindow.SetPlacement( placement );
                }
                else
                {
                    int w;
                    int h;
                    var viewPortSize = Config[_ctxVm.KeyboardContext.CurrentKeyboard.CurrentLayout]["ViewPortSize"];
                    if( viewPortSize != null )
                    {
                        Size size = (Size)viewPortSize;
                        w = (int)size.Width;
                        h = (int)size.Height;
                    }
                    else
                    {
                        w = _ctxVm.KeyboardVM.W;
                        h = _ctxVm.KeyboardVM.H;
                    }

                    SetDefaultWindowPosition( w, h );
                }
            }
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
            if( _autohide )
            {
                if( _timer == null )
                    _timer = new DispatcherTimer( TimeSpan.FromMilliseconds( timeout ), DispatcherPriority.Normal, ( o, args ) => HideSkin(), _skinWindow.Dispatcher );
                else
                    _timer.Interval = TimeSpan.FromMilliseconds( timeout );

                _timer.Start();
            }
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

        void OnSelectElement( object sender, HighlightEventArgs e )
        {

            if( e.Element is VMKeySimple )
            {
                VMKeySimple key = (VMKeySimple)e.Element;
                if( key.KeyDownCommand.CanExecute( null ) )
                {
                    key.KeyDownCommand.Execute( null );
                    if( key.KeyUpCommand.CanExecute( null ) )
                    {
                        key.KeyUpCommand.Execute( null );
                    }
                }
            }
        }

        void OnBeginHighlight( object sender, HighlightEventArgs e )
        {
            VMZoneSimple vm = e.Element as VMZoneSimple;
            if( vm != null ) vm.IsHighlighting = true;
            else
            {
                VMKeySimple vmk = e.Element as VMKeySimple;
                if( vmk != null ) vmk.IsHighlighting = true;
            }

        }

        void OnEndHighlight( object sender, HighlightEventArgs e )
        {
            VMZoneSimple vm = e.Element as VMZoneSimple;
            if( vm != null ) vm.IsHighlighting = false;
            else
            {
                VMKeySimple vmk = e.Element as VMKeySimple;
                if( vmk != null ) vmk.IsHighlighting = false;
            }
        }

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
            Highlighter.Service.UnregisterTree( _ctxVm.KeyboardVM );
            Highlighter.Service.BeginHighlight -= OnBeginHighlight;
            Highlighter.Service.EndHighlight -= OnEndHighlight;
            Highlighter.Service.SelectElement -= OnSelectElement;
        }

        private void RegisterHighlighter()
        {
            Highlighter.Service.RegisterTree( _ctxVm.KeyboardVM );
            Highlighter.Service.BeginHighlight += OnBeginHighlight;
            Highlighter.Service.EndHighlight += OnEndHighlight;
            Highlighter.Service.SelectElement += OnSelectElement;
        }

        #endregion

        #region MiniView methods & properties

        /// <summary>
        /// Toggles minimization of the application's host, the configuration window.
        /// </summary>
        public void ToggleHostMinimized()
        {
            //TODOJL
            //HostManipulator.ToggleMinimize( _skinWindow.LastFocusedWindowHandle );
        }

        /// <summary>
        /// Gets whether the application's host's window is minimized.
        /// </summary>
        public bool IsHostMinimized { get { return HostManipulator.IsMinimized; } }

        /// <summary>
        /// Hides the skin and shows the keyboard's MiniView
        /// </summary>
        public void HideSkin()
        {
            if( !_viewHidden )
            {
                _viewHidden = true;
                _skinWindow.Hide();
                ShowMiniView();

                if( Highlighter.Status == InternalRunningStatus.Started )
                {
                    Highlighter.Service.RegisterTree( _miniViewVm );
                    Highlighter.Service.UnregisterTree( _ctxVm.KeyboardVM );
                }

                if( _timer != null ) _timer.Stop();
            }
        }

        /// <summary>
        /// Hides the keyboard's MiniView and shows the keyboard
        /// </summary>
        public void RestoreSkin()
        {
            if( _viewHidden )
            {
                _viewHidden = false;
                _miniView.Hide();

                _skinWindow.Show();
                if( Highlighter.Status == InternalRunningStatus.Started )
                {
                    Highlighter.Service.RegisterTree( _ctxVm.KeyboardVM );
                    Highlighter.Service.UnregisterTree( _miniViewVm );
                }
            }
        }

        void ShowMiniView()
        {
            if( _miniView == null )
            {
                _miniViewVm = new MiniViewVM( this );
                _miniView = new MiniView( RestoreSkin ) { DataContext = _miniViewVm };
                _miniView.Closing += new CancelEventHandler( OnWindowClosing );

                _miniView.Show();

                if( !ScreenHelper.IsInScreen( new System.Drawing.Point( (int)( _miniViewVm.X + (int)_miniView.ActualWidth / 2 ), _miniViewVm.Y + (int)_miniView.ActualHeight / 2 ) ) ||
                    !ScreenHelper.IsInScreen( new System.Drawing.Point( (int)( _miniViewVm.X + (int)_miniView.ActualWidth ), _miniViewVm.Y + (int)_miniView.ActualHeight ) ) )
                {
                    _miniView.Left = 0;
                    _miniView.Top = 0;
                }
            }
            else
                _miniView.Show();
        }

        #endregion
    }

    public class MiniViewVM : VMBase, IHighlightableElement, IDisposable
    {
        public SimpleSkin Parent { get; set; }
        IPluginConfigAccessor Config { get { return Parent.Config; } }

        bool _isHighlighted;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set { _isHighlighted = value; OnPropertyChanged( "IsHighlighted" ); }
        }

        public MiniViewVM( SimpleSkin parent )
        {
            _isHighlighted = false;

            Parent = parent;


            if( Parent.Highlighter.Status == InternalRunningStatus.Started )
            {
                Parent.Highlighter.Service.SelectElement += OnSelectElement;
                Parent.Highlighter.Service.BeginHighlight += OnBeginHighlight;
                Parent.Highlighter.Service.EndHighlight += OnEndHighlight;
            }
            Parent.Highlighter.ServiceStatusChanged += OnHighlighterServiceStatusChanged;
        }

        void OnBeginHighlight( object sender, HighlightEventArgs e )
        {
            if( Parent.IsViewHidden && e.Element == this )
            {
                IsHighlighted = true;
            }
        }

        void OnEndHighlight( object sender, HighlightEventArgs e )
        {
            if( Parent.IsViewHidden && e.Element == this )
            {
                IsHighlighted = false;
            }
        }

        void OnSelectElement( object sender, HighlightEventArgs e )
        {
            if( Parent.IsViewHidden && e.Element == this )
            {
                Parent.RestoreSkin();
            }
        }

        void OnHighlighterServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                Parent.Highlighter.Service.BeginHighlight += OnBeginHighlight;
                Parent.Highlighter.Service.EndHighlight += OnEndHighlight;
                Parent.Highlighter.Service.SelectElement += OnSelectElement;
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                Parent.Highlighter.Service.BeginHighlight -= OnBeginHighlight;
                Parent.Highlighter.Service.EndHighlight -= OnEndHighlight;
                Parent.Highlighter.Service.SelectElement -= OnSelectElement;
            }
        }

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return CKReadOnlyListEmpty<IHighlightableElement>.Empty; }
        }

        public int X
        {
            get
            {
                var position = Config.Context["MiniViewPositionX"];
                if( position == null )
                {
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
                    System.Drawing.Point p = ScreenHelper.GetCenterOfParentScreen( rect );

                    return p.X;
                }
                else
                    return ( Int32.Parse( position.ToString() ) );
            }
            set { Config.Context["MiniViewPositionX"] = value; }
        }

        public int Y
        {
            get
            {
                var position = Config.Context["MiniViewPositionY"];
                if( position == null )
                    return 0;
                else
                    return ( Int32.Parse( position.ToString() ) );

            }
            set { Config.Context["MiniViewPositionY"] = value; }
        }

        int _width = 160;
        public int Width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged( "Width" ); }
        }

        int _height = 160;
        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged( "Height" );
            }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.None; }
        }

        public void Dispose()
        {
            Parent.Highlighter.ServiceStatusChanged -= OnHighlighterServiceStatusChanged;
            if( Parent.Highlighter.Status == InternalRunningStatus.Started )
            {
                Parent.Highlighter.Service.SelectElement -= OnSelectElement;
                Parent.Highlighter.Service.BeginHighlight -= OnBeginHighlight;
                Parent.Highlighter.Service.EndHighlight -= OnEndHighlight;
            }
        }


    }

}
