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

namespace SimpleSkin
{
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

        IHostManipulator _hostManipulator;
        VMContextSimple _ctxVm;
        SkinWindow _skinWindow;
        DispatcherTimer _timer;
        MiniView _miniView;
        bool _forceClose;
        bool _viewHidden;
        bool _isStarted;
        bool _autohide;
        int _timeout;

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        [RequiredService]
        public INotificationService Notification { get; set; }

        //Since the IHotsManipulator implementaiton is pushed to the servicecontainer after plugins are discovered and loaded, we cant use the RequiredService tag to fetch a ref to the HostManipulator.
        /// <summary>
        /// The HostManipulator, enables minimizing the host.
        /// </summary>
        public IHostManipulator HostManipulator { get { return _hostManipulator ?? ( _hostManipulator = Context.ServiceContainer.GetService<IHostManipulator>() ); } }

        /// <summary>
        /// Acessor to the CK config object.
        /// </summary>
        public IPluginConfigAccessor Config { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
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

        public void Start()
        {
            if( KeyboardContext.Status == RunningStatus.Started && KeyboardContext.Service.Keyboards.Count > 0 )
            {
                Context.ServiceContainer.Add( Config );

                _isStarted = true;
                _ctxVm = new VMContextSimple( Context, KeyboardContext.Service, Config );
                _skinWindow = new SkinWindow( _ctxVm );

                Highlighter.ServiceStatusChanged += OnHighlighterServiceStatusChanged;
                if( Highlighter.Status == RunningStatus.Started )
                {
                    Highlighter.Service.RegisterTree( _ctxVm.KeyboardVM );
                    Highlighter.Service.BeginHighlight += OnBeginHighlight;
                    Highlighter.Service.EndHighlight += OnEndHighlight;
                    Highlighter.Service.SelectElement += OnSelectElement;
                }

                int defaultWidth = _ctxVm.KeyboardVM.W;
                int defaultHeight = _ctxVm.KeyboardVM.H;

                if( !Config.User.Contains( PlacementString ) ) SetDefaultWindowPosition( defaultWidth, defaultHeight ); //first launch : places the skin in the default position
                else _skinWindow.Width = _skinWindow.Height = 0; //After the first launch : hiding the window to get its last placement from the user conf.

                _skinWindow.Show();

                if( !Config.User.Contains( PlacementString ) ) Config.User.Set( PlacementString, _skinWindow.GetPlacement() );

                //Placing the skin at the same location as the last launch.
                _skinWindow.SetPlacement( (WINDOWPLACEMENT)Config.User[PlacementString] );

                //autohide
                UpdateAutoHideConfig();

                Config.ConfigChanged += new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );

                RegisterEvents();
            }
            else
            {
                _isStarted = false;
                Notification.ShowNotification( PluginId.UniqueId, "Aucun clavier n'est disponible",
                    "Aucun clavier n'est disponible dans le contexte actuel, veuillez choisir un contexte contenant au moins un clavier.", 1000, NotificationTypes.Error );
            }
        }

        void OnSelectElement( object sender, HighlightEventArgs e )
        {
            if( e.Element is VMKeySimple ) {
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
            if( e.Current == RunningStatus.Started )
            {
                Highlighter.Service.RegisterTree( _ctxVm.KeyboardVM );
                Highlighter.Service.BeginHighlight += OnBeginHighlight;
                Highlighter.Service.EndHighlight += OnEndHighlight;
                Highlighter.Service.SelectElement += OnSelectElement;
            }
            else if( e.Current == RunningStatus.Stopping )
            {
                Highlighter.Service.UnregisterTree( _ctxVm.KeyboardVM );
                Highlighter.Service.BeginHighlight -= OnBeginHighlight;
                Highlighter.Service.EndHighlight -= OnEndHighlight;
                Highlighter.Service.SelectElement -= OnSelectElement;
            }
        }

        private void RegisterEvents()
        {
            _skinWindow.Closing += new CancelEventHandler( OnWindowClosing );
            _skinWindow.MouseLeave += new System.Windows.Input.MouseEventHandler( OnMouseLeaveWindow );
            _skinWindow.MouseEnter += new System.Windows.Input.MouseEventHandler( OnMouseEnterWindow );
            _skinWindow.SizeChanged += new SizeChangedEventHandler( OnWindowResized );
            _ctxVm.KeyboardContext.CurrentKeyboardChanging += new EventHandler<CurrentKeyboardChangingEventArgs>( OnCurrentKeyboardChanging );
            _ctxVm.KeyboardContext.CurrentKeyboardChanged += new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
        }

        void OnCurrentKeyboardChanging( object sender, CurrentKeyboardChangingEventArgs e )
        {
            if( Highlighter.Status == RunningStatus.Started )
            {
                Highlighter.Service.UnregisterTree( _ctxVm.KeyboardVM );
            }
             
            //Saving the state of the window before doing anything (if the current keyboard is not null)
            if( e.Current != null && _skinWindow != null )
            {
                Config.User.Set( PlacementString, _skinWindow.GetPlacement() );
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
            if( Highlighter.Status == RunningStatus.Started )
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
                else SetDefaultWindowPosition( _ctxVm.KeyboardVM.W, _ctxVm.KeyboardVM.H );
            }
        }

        private void UnregisterEvents()
        {
            _ctxVm.KeyboardContext.CurrentKeyboardChanging -= new EventHandler<CurrentKeyboardChangingEventArgs>( OnCurrentKeyboardChanging );
            _ctxVm.KeyboardContext.CurrentKeyboardChanged -= new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
            _skinWindow.Closing -= new CancelEventHandler( OnWindowClosing );
            _skinWindow.MouseLeave -= new System.Windows.Input.MouseEventHandler( OnMouseLeaveWindow );
            _skinWindow.MouseEnter -= new System.Windows.Input.MouseEventHandler( OnMouseEnterWindow );
            _skinWindow.SizeChanged -= new SizeChangedEventHandler( OnWindowResized );
        }

        private void SetDefaultWindowPosition( int defaultWidth, int defaultHeight )
        {
            _skinWindow.Top = 0;
            _skinWindow.Left = 0;
            _skinWindow.Width = defaultWidth;
            _skinWindow.Height = defaultHeight;
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( ( c ) => c.UniqueId.Equals( this.PluginGuid ) ) && !String.IsNullOrEmpty( e.Key ) )
            {
                if( e.Key == "autohide" || e.Key == "autohide-timeout" ) UpdateAutoHideConfig();
            }
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

        void OnMouseLeaveWindow( object sender, System.Windows.Input.MouseEventArgs e )
        {
            int timeout = _timeout == 0 ? 6000 : _timeout;
            if( _autohide )
            {
                if( _timer == null )
                    _timer = new DispatcherTimer( TimeSpan.FromMilliseconds( timeout ), DispatcherPriority.Normal, ( o, args ) => Hide(), _skinWindow.Dispatcher );
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

        public void Stop()
        {
            if( _isStarted )
            {
                Highlighter.Service.UnregisterTree( _ctxVm.KeyboardVM );
                Context.ServiceContainer.Remove( typeof( IPluginConfigAccessor ) );

                UnregisterEvents();

                Config.User.Set( PlacementString, _skinWindow.GetPlacement() );

                _forceClose = true;
                _skinWindow.Close();

                if( _miniView != null )
                {
                    _miniView.Close();
                    _miniView = null;
                    _viewHidden = false;
                }

                _ctxVm.Dispose();
                _ctxVm = null;
                _isStarted = false;
            }
        }

        public void Teardown()
        {
        }

        /// <summary>
        /// Toggles minimization of the application's host, the configuration window.
        /// </summary>
        public void ToggleHostMinimized()
        {
            HostManipulator.ToggleMinimize( _skinWindow.LastFocusedWindowHandle );
        }

        /// <summary>
        /// Gets whether the application's host's window is minimized.
        /// </summary>
        public bool IsHostMinimized { get { return HostManipulator.IsMinimized; } }

        /// <summary>
        /// Hides the skin and shows the keyboard's MiniView
        /// </summary>
        public void Hide()
        {
            if( !_viewHidden )
            {
                _viewHidden = true;
                _skinWindow.Hide();
                ShowMiniView();

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
            }
        }

        /// <summary>
        /// Gets or sets the MiniView's X position
        /// </summary>
        public double MiniViewPositionX
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
                    return (double)position;
            }
            set { Config.Context["MiniViewPositionX"] = value; }
        }

        /// <summary>
        /// Gets or sets the MiniView's Y position
        /// </summary>
        public double MiniViewPositionY
        {
            get
            {
                var position = Config.Context["MiniViewPositionY"];
                if( position == null )
                    return 0;
                else
                    return (double)position;

            }
            set { Config.Context["MiniViewPositionY"] = value; }
        }

        void ShowMiniView()
        {
            if( _miniView == null )
            {
                _miniView = new MiniView( RestoreSkin ) { DataContext = this };
                _miniView.Closing += new CancelEventHandler( OnWindowClosing );

                _miniView.Show();

                if( !ScreenHelper.IsInScreen( new System.Drawing.Point( (int)( MiniViewPositionX + (int)_miniView.ActualWidth / 2 ), (int)MiniViewPositionY + (int)_miniView.ActualHeight / 2 ) ) ||
                    !ScreenHelper.IsInScreen( new System.Drawing.Point( (int)( MiniViewPositionX + (int)_miniView.ActualWidth ), (int)MiniViewPositionY + (int)_miniView.ActualHeight ) ) )
                {
                    _miniView.Left = 0;
                    _miniView.Top = 0;
                }
            }
            else
                _miniView.Show();
        }
    }
}
