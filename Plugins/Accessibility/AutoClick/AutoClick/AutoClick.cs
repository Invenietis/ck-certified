#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\AutoClick.cs) is part of CiviKey. 
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
using System.Linq;
using CK.Plugin;
using System.Windows.Input;
using CK.Plugins.AutoClick.Views;
using CommonServices;
using CK.Plugins.AutoClick.Model;
using CK.WPF.ViewModel;
using System.Windows;
using CK.Plugin.Config;
using CK.Core;
using System.ComponentModel;
using CK.Windows;
using CK.Windows.Helpers;
using CK.WindowManager.Model;
using System.IO;
using Help.Services;
using CK.InputDriver;
using CK.InputDriver.Hook;

namespace CK.Plugins.AutoClick
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class AutoClick : VMBase, IPlugin, IHaveDefaultHelp
    {
        const string PluginGuidString = "{989BE0E6-D710-489e-918F-FBB8700E2BB2}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "AutoClick Plugin";
        public readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginIdVersion, PluginPublicName );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> MouseDriver { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IMouseWatcher> MouseWatcher { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IHelpViewerService> HelpService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IClickSelector> Selector { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ITopMostService> TopMostService { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        private MouseDecoratorWindow _mouseIndicatorWindow;
        private AutoClickEditorWindow _editorWindow;
        private AutoClickWindow _autoClickWindow;

        public bool IsEditorOpened { get { return _editorWindow.IsVisible; } }

        bool _isPaused;
        public bool IsPaused
        {
            get { return _isPaused; }
            set { _isPaused = value; OnPropertyChanged( "IsPaused" ); OnPropertyChanged( "ShowMouseIndicator" ); }
        }

        //true if the user wants to see the indicator next to the mouse pointer
        public bool ShowMouseIndicatorOption
        {
            get { return (bool)Config.User["ShowMouseIndicatorOption"]; }
            set { Config.User["ShowMouseIndicatorOption"] = value; }
        }

        //true if the user wants to see the MousePanel and the autoclick is not paused
        public bool ShowMouseIndicator
        {
            get { return !_isPaused && ShowMouseIndicatorOption; }
        }

        /// <summary>
        /// Progression of the click cycle ( 0 to 100)
        /// </summary>
        public int ProgressValue
        {
            get { return MouseWatcher.Service.ProgressValue; }
        }

        public int TimeBeforeCountDownStarts
        {
            get { return (int)Config.User["TimeBeforeCountDownStarts"]; }
        }

        public int CountDownDuration
        {
            get { return (int)Config.User["CountDownDuration"]; }
        }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _isPaused = true;
            _autoClickWindow = new AutoClickWindow() { DataContext = this };

            int defaultHeight = (int)( System.Windows.SystemParameters.WorkArea.Width ) / 10;
            int defaultWidth = defaultHeight / 2;

            if( !Config.User.Contains( "AutoClickWindowPlacement" ) )
            {
                SetDefaultWindowPosition( defaultWidth, defaultHeight );
            }
            else
            {
                _autoClickWindow.Width = _autoClickWindow.Height = 0;
            }

            _mouseIndicatorWindow = new MouseDecoratorWindow { DataContext = this };
            _editorWindow = new AutoClickEditorWindow { DataContext = this };

            Config.ConfigChanged += new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );
            ConfigureMouseWatcher();
            RegisterEvents();

            Config.User.GetOrSet<bool>( "ShowMouseIndicatorOption", true );

            _mouseIndicatorWindow.Show();
            _autoClickWindow.Show();

            //Executed only at first launch, has to be done once the window is shown, otherwise, it will save a "hidden" state for the window
            if( !Config.User.Contains( "AutoClickWindowPlacement" ) ) Config.User.Set( "AutoClickWindowPlacement", CKWindowTools.GetPlacement( _autoClickWindow.Hwnd ) );
            CKWindowTools.SetPlacement( _autoClickWindow.Hwnd, (WINDOWPLACEMENT)Config.User["AutoClickWindowPlacement"] );

            //Re-positions the window in the screen if it is not in it. Which may happen if the autoclick is saved as being on a secondary screen.
            if( !ScreenHelper.IsInScreen( new System.Drawing.Point( (int)_autoClickWindow.Left, (int)_autoClickWindow.Top ) )
                && !ScreenHelper.IsInScreen( new System.Drawing.Point( (int)(_autoClickWindow.Left + _autoClickWindow.ActualWidth), (int)_autoClickWindow.Top ) ) )
            {
                SetDefaultWindowPosition( defaultWidth, defaultHeight );
            }

            WindowManager.Service.RegisterWindow( "AutoClick", _autoClickWindow );
            TopMostService.Service.RegisterTopMostElement( "10", _autoClickWindow );
            WindowBinder.Service.Bind( WindowManager.Service.GetByName( "AutoClick" ), WindowManager.Service.GetByName( "ClickSelector" ), BindingPosition.Bottom );

            OnPause( this, EventArgs.Empty );
        }

        private void SetDefaultWindowPosition( int defaultWidth, int defaultHeight )
        {
            _autoClickWindow.Top = 0;
            _autoClickWindow.Left = (int)System.Windows.SystemParameters.WorkArea.Width - defaultWidth;
            _autoClickWindow.Width = defaultWidth;
            _autoClickWindow.Height = defaultHeight;
        }

        public void Stop()
        {
            Config.ConfigChanged -= new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );
            UnregisterEvents();
            Config.User.Set( "AutoClickWindowPlacement", CKWindowTools.GetPlacement( _autoClickWindow.Hwnd ) );
        }

        public void Teardown()
        {
            if( _mouseIndicatorWindow != null )
            {
                _mouseIndicatorWindow.Close();
                _editorWindow.Close();
            }

            if( _autoClickWindow != null )
            {
                if( _autoClickWindow.IsVisible ) _autoClickWindow.Close();
            }

            _editorWindow = null;
            _mouseIndicatorWindow = null;
            _autoClickWindow = null;
        }

        #endregion

        #region EventHandlers

        /// <summary>
        /// Launches OnPropertyChanged when needed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( ( c ) => c.UniqueId.Equals( this.PluginGuid ) ) && !String.IsNullOrEmpty( e.Key ) )
            {
                switch( e.Key )
                {
                    case "TimeBeforeCountDownStarts":
                        MouseWatcher.Service.TimeBeforeCountDownStarts = (int)e.Value;
                        break;
                    case "CountDownDuration":
                        MouseWatcher.Service.CountDownDuration = (int)e.Value;
                        break;
                    case "ShowMouseIndicatorOption":
                        OnPropertyChanged( "ShowMouseIndicator" );
                        break;
                    default:
                        return;
                }

                OnPropertyChanged( e.Key );
            }
        }

        void OnMouseDriverServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Stopped )
            {
                MouseDriver.Service.PointerMove -= new PointerDeviceEventHandler( OnPointerMove );
            }
        }

        void OnPointerMove( object sender, PointerDeviceEventArgs e )
        {
            _mouseIndicatorWindow.Left = e.X + 10;
            _mouseIndicatorWindow.Top = e.Y - 20;
        }

        private void OnEditorWindowVisibilityChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            OnPropertyChanged( "IsEditorOpened" );
        }

        private void OnClickCancelled( object sender, EventArgs e )
        {
            OnPropertyChanged( "ProgressValue" );
            //TODO Nice animation to show that the click has failed ?
        }

        private void OnHasPaused( object sender, EventArgs e )
        {
            _isPaused = true;
            OnPropertyChanged( "ProgressValue" );
            OnPropertyChanged( "ShowMouseIndicator" );
        }

        private void OnHasResumed( object sender, EventArgs e )
        {
            _isPaused = false;
            OnPropertyChanged( "ShowMouseIndicator" );
        }

        private void OnProgressValueChanged( object sender, AutoClickProgressValueChangedEventArgs e )
        {
            OnPropertyChanged( "ProgressValue" );
        }

        private void OnClickAsked( object sender, EventArgs e )
        {
            OnPropertyChanged( "ProgressValue" );
            //Asking for a click, the IClickSelector will respond via the ClickTypeChosenEvent
            Selector.Service.AskClickType();
        }

        private void OnMouseWatcherPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            OnPropertyChanged( e.PropertyName );
        }

        #endregion

        #region Methods

        private void RegisterEvents()
        {
            MouseDriver.Service.PointerMove += OnPointerMove;
            
            MouseWatcher.Service.LaunchClick += OnClickAsked;
            MouseWatcher.Service.ProgressValueChanged += OnProgressValueChanged;
            MouseWatcher.Service.ClickCanceled += OnClickCancelled;
            MouseWatcher.Service.HasPaused += OnHasPaused;
            MouseWatcher.Service.HasResumed += OnHasResumed;
            MouseWatcher.Service.PropertyChanged += OnMouseWatcherPropertyChanged;
            Selector.Service.ClickChosen += OnClickChosen;
            Selector.Service.ResumeEvent += OnResume;
            Selector.Service.StopEvent += OnPause;

            MouseDriver.ServiceStatusChanged += OnMouseDriverServiceStatusChanged;
        }

        private void UnregisterEvents()
        {
            if( MouseDriver.Status != InternalRunningStatus.Stopped && MouseDriver.Status != InternalRunningStatus.Disabled )
                MouseDriver.Service.PointerMove -= OnPointerMove;

            if( MouseWatcher.Status != InternalRunningStatus.Stopped && MouseWatcher.Status != InternalRunningStatus.Disabled )
            {
                MouseWatcher.Service.LaunchClick -= OnClickAsked;
                MouseWatcher.Service.ProgressValueChanged -= OnProgressValueChanged;
                MouseWatcher.Service.ClickCanceled -= OnClickCancelled;
                MouseWatcher.Service.HasPaused -= OnHasPaused;
                MouseWatcher.Service.HasResumed -= OnHasResumed;
                MouseWatcher.Service.PropertyChanged -= OnMouseWatcherPropertyChanged;
            }

            Selector.Service.ClickChosen -= OnClickChosen;
            Selector.Service.ResumeEvent -= OnResume;
            Selector.Service.StopEvent -= OnPause;
            _editorWindow.IsVisibleChanged -= OnEditorWindowVisibilityChanged;
        }

        private void ConfigureMouseWatcher()
        {
            if( Config.User["TimeBeforeCountDownStarts"] == null )
                Config.User["TimeBeforeCountDownStarts"] = 1500;

            if( Config.User["CountDownDuration"] == null )
                Config.User["CountDownDuration"] = 2000;

            MouseWatcher.Service.TimeBeforeCountDownStarts = (int)Config.User["TimeBeforeCountDownStarts"];
            MouseWatcher.Service.CountDownDuration = (int)Config.User["CountDownDuration"];
        }

        internal void ModifyCountDownConfiguration( string property, int value )
        {
            if( property == "CountDownDuration" )
            {
                int newValue = CountDownDuration;
                if( CountDownDuration + value > 0 )
                    newValue = CountDownDuration + value;
                Config.User[property] = newValue;
            }
            else if( property == "TimeBeforeCountDownStarts" )
            {
                int newValue = TimeBeforeCountDownStarts;
                if( TimeBeforeCountDownStarts + value > 0 )
                    newValue = TimeBeforeCountDownStarts + value;
                Config.User[property] = newValue;
            }
            OnPropertyChanged( property );
        }

        /// <summary>
        /// Callback method that is called when the IClickSelector has sent a Click (will send the actual click)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickChosen( object sender, ClickTypeEventArgs e )
        {
            foreach( ClickInstruction instr in e.ClickVM )
            {
                switch( instr )
                {
                    case ClickInstruction.None:
                        break;
                    case ClickInstruction.RightButtonDown:
                        MouseProcessor.RightButtonDown();
                        break;
                    case ClickInstruction.RightButtonUp:
                        MouseProcessor.RightButtonUp();
                        break;
                    case ClickInstruction.LeftButtonDown:
                        MouseProcessor.LeftButtonDown();
                        break;
                    case ClickInstruction.LeftButtonUp:
                        MouseProcessor.LeftButtonUp();
                        break;
                    case ClickInstruction.WheelDown:
                        MouseProcessor.MiddleButtonDown();
                        break;
                    case ClickInstruction.WheelUp:
                        MouseProcessor.MiddleButtonUp();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Method that is called when the AutoClickPlugin can start over its work
        /// </summary>
        /// <param name="sender"></param>
        public void OnResume( object sender, EventArgs e )
        {
            MouseWatcher.Service.Resume();
            IsPaused = false;
        }

        /// <summary>
        /// Method that is called when the AutoClickPlugin must stop
        /// </summary>
        /// <param name="sender"></param>
        public void OnPause( object sender, EventArgs e )
        {
            MouseWatcher.Service.Pause();
            IsPaused = true;
        }

        #endregion

        #region Commands

        private ICommand _showHelpCommand;
        public ICommand ShowHelpCommand
        {
            get
            {
                return _showHelpCommand ?? (_showHelpCommand = new CK.Windows.App.VMCommand( () =>
                {
                    if( HelpService.Status == InternalRunningStatus.Started )
                    {
                        HelpService.Service.ShowHelpFor( PluginId, true );
                    }
                } ));
            }
        }

        private ICommand _toggleEditorVisibilityCommand;
        public ICommand ToggleEditorVisibilityCommand
        {
            get
            {
                if( _toggleEditorVisibilityCommand == null )
                {
                    _toggleEditorVisibilityCommand = new CK.Windows.App.VMCommand( () =>
                    {
                        //Ugly fix, waiting for me to really understand how Show/Close/Visiblity work
                        double editorWindowWidth = _editorWindow.ActualWidth == 0 ? _editorWindow.Width : _editorWindow.ActualWidth;

                        double editorLeft = _autoClickWindow.Left - editorWindowWidth;
                        if( editorLeft > 0 && editorLeft + _editorWindow.ActualWidth < System.Windows.SystemParameters.PrimaryScreenWidth )
                            _editorWindow.Left = editorLeft;
                        else
                            _editorWindow.Left = _autoClickWindow.Left + _autoClickWindow.ActualWidth;

                        _editorWindow.Top = _autoClickWindow.Top;

                        if( !_editorWindow.IsVisible ) _editorWindow.Visibility = Visibility.Visible;
                        else _editorWindow.Visibility = Visibility.Hidden;

                        OnPropertyChanged( "IsEditorOpened" );
                    } );
                }
                return _toggleEditorVisibilityCommand;
            }
        }

        private ICommand _togglePauseCommand;
        public ICommand TogglePauseCommand
        {
            get
            {
                if( _togglePauseCommand == null )
                {
                    _togglePauseCommand = new CK.Windows.App.VMCommand( () => { if( IsPaused ) OnResume( this, EventArgs.Empty ); else OnPause( this, EventArgs.Empty ); } );
                }

                return _togglePauseCommand;
            }
        }

        private ICommand _incrementTimeBeforeCountDownStartsCommand;
        public ICommand IncrementTimeBeforeCountDownStartsCommand
        {
            get
            {
                if( _incrementTimeBeforeCountDownStartsCommand == null )
                {
                    _incrementTimeBeforeCountDownStartsCommand = new CK.Windows.App.VMCommand( () => ModifyCountDownConfiguration( "TimeBeforeCountDownStarts", 100 ) );
                }
                return _incrementTimeBeforeCountDownStartsCommand;
            }
        }

        private ICommand _decrementTimeBeforeCountDownStartsCommand;
        public ICommand DecrementTimeBeforeCountDownStartsCommand
        {
            get
            {
                if( _decrementTimeBeforeCountDownStartsCommand == null )
                {
                    _decrementTimeBeforeCountDownStartsCommand = new CK.Windows.App.VMCommand( () => ModifyCountDownConfiguration( "TimeBeforeCountDownStarts", -100 ) );
                }
                return _decrementTimeBeforeCountDownStartsCommand;
            }
        }

        private ICommand _incrementCountDownDurationCommand;
        public ICommand IncrementCountDownDurationCommand
        {
            get
            {
                if( _incrementCountDownDurationCommand == null )
                {
                    _incrementCountDownDurationCommand = new CK.Windows.App.VMCommand( () => ModifyCountDownConfiguration( "CountDownDuration", 100 ) );
                }
                return _incrementCountDownDurationCommand;
            }
        }

        private ICommand _decrementCountDownDurationCommand;
        public ICommand DecrementCountDownDurationCommand
        {
            get
            {
                if( _decrementCountDownDurationCommand == null )
                {
                    _decrementCountDownDurationCommand = new CK.Windows.App.VMCommand( () => ModifyCountDownConfiguration( "CountDownDuration", -100 ) );
                }
                return _decrementCountDownDurationCommand;
            }
        }

        #endregion

        #region IHaveDefaultHelp Members

        public Stream GetDefaultHelp()
        {
            return typeof( AutoClick ).Assembly.GetManifestResourceStream( "AutoClick.Res.helpcontent.zip" );
        }

        #endregion
    }
}