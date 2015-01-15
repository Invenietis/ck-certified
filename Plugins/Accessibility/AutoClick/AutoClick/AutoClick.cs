﻿#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\AutoClick\AutoClick.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CK.Core;
using CK.InputDriver;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Plugins.AutoClick.Model;
using CK.Plugins.AutoClick.Views;
using CK.WindowManager.Model;
using CK.WPF.ViewModel;
using CommonServices;
using Help.Services;

namespace CK.Plugins.AutoClick
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion )]
    public class AutoClick : VMBase, IPlugin, IHaveDefaultHelp
    {
        #region Plugin description

        const string PluginGuidString = "{989BE0E6-D710-489e-918F-FBB8700E2BB2}";
        const string PluginVersion = "1.0.0";
        const string PluginPublicName = "AutoClick Plugin";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> MouseDriver { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IMouseWatcher> MouseWatcher { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IClickSelector> Selector { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ISharedData> SharedData { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IHelpViewerService> HelpService { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ITopMostService> TopMostService { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        private MouseDecoratorWindow _mouseIndicatorWindow;
        private AutoClickEditorWindow _editorWindow;
        private AutoClickWindow _autoClickWindow;

        public bool IsEditorOpened { get { return _editorWindow.IsVisible; } }

        bool _isClosing;
        bool _isPaused;
        bool _mouseIndicatorIsEnable;
        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                if( _isPaused != value )
                {
                    _isPaused = value;
                    MouseIndicatorIsEnable = !value;
                    OnPropertyChanged( "IsPaused" );
                    OnPropertyChanged( "ShowMouseIndicator" );
                }
            }
        }

        public bool MouseIndicatorIsEnable
        {
            get { return _mouseIndicatorIsEnable; }
            set 
            {
                if( _mouseIndicatorIsEnable != value )
                {
                    _mouseIndicatorIsEnable = value;
                    OnPropertyChanged( "MouseIndicatorIsEnable" );
                    OnPropertyChanged( "ShowMouseIndicator" );

                    if( _mouseIndicatorIsEnable )
                    {
                        MouseDriver.Service.PointerMove += OnPointerMove;
                        SetPointerPosition( MouseDriver.Service.CurrentPointerXLocation, MouseDriver.Service.CurrentPointerYLocation );
                    }
                    else
                    {
                        MouseDriver.Service.PointerMove -= OnPointerMove;
                    }
                }
            }
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
            get { return !IsPaused && MouseIndicatorIsEnable && ShowMouseIndicatorOption; }
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


        int defaultHeight;
        int defaultWidth;

        public bool Setup( IPluginSetupInfo info )
        {
            _isPaused = true;
            _mouseIndicatorIsEnable = false;
            _isClosing = false;

            defaultHeight = (int)(System.Windows.SystemParameters.WorkArea.Width) / 10;
            defaultWidth = defaultHeight / 2;

            return true;
        }

        public void Start()
        {
            InitializeSharedData();
            _autoClickWindow = new AutoClickWindow() { DataContext = this };
            _autoClickWindow.Closing += OnWindowClosing;

            _mouseIndicatorWindow = new MouseDecoratorWindow { DataContext = this };
            _editorWindow = new AutoClickEditorWindow { DataContext = this };

            Config.ConfigChanged += new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );
            ConfigureMouseWatcher();
            RegisterEvents();

            Config.User.GetOrSet<bool>( "ShowMouseIndicatorOption", true );

            _mouseIndicatorWindow.Show();
            _autoClickWindow.Show();

            OnPause( this, EventArgs.Empty );

            InitializeWindowManager();
            InitializeTopMost();
        }

        void OnWindowClosing( object sender, CancelEventArgs e )
        {
            if( !_isClosing )
                e.Cancel = true;
        }

        public void Stop()
        {
            _isClosing = true;

            UninitializeTopMost();
            UninitializeWindowManager();

            Config.ConfigChanged -= new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );

            UnregisterEvents();
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

        #region IWindowManager Members

        void InitializeWindowManager()
        {
            RegisterWindowManager();
            WindowManager.ServiceStatusChanged += OnWindowManagerStatusChanged;
        }

        void UninitializeWindowManager()
        {
            WindowManager.ServiceStatusChanged -= OnWindowManagerStatusChanged;
            UnregisterWindowManager();
        }

        void OnWindowManagerStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                WindowManager.Service.RegisterWindow( "AutoClick", _autoClickWindow );
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                WindowManager.Service.UnregisterWindow( "AutoClick" );
            }
        }

        void RegisterWindowManager()
        {
            if( WindowManager.Status.IsStartingOrStarted ) WindowManager.Service.RegisterWindow( "AutoClick", _autoClickWindow );
        }

        void UnregisterWindowManager()
        {
            if( WindowManager.Status.IsStartingOrStarted ) WindowManager.Service.UnregisterWindow( "AutoClick" );
        }

        #endregion IWindowManager Members

        #region ITopMostService Members

        void InitializeTopMost()
        {
            RegisterTopMost();
            TopMostService.ServiceStatusChanged += OnTopMostServiceStatusChanged;
        }

        void UninitializeTopMost()
        {
            TopMostService.ServiceStatusChanged -= OnTopMostServiceStatusChanged;
            UnregisterTopMost();
        }

        void RegisterTopMost()
        {
            if( TopMostService.Status.IsStartingOrStarted )
            {
                TopMostService.Service.RegisterTopMostElement( "10", _autoClickWindow );
                TopMostService.Service.RegisterTopMostElement( "200", _mouseIndicatorWindow );
            }
        }

        void UnregisterTopMost()
        {
            if( TopMostService.Status.IsStartingOrStarted )
            {
                TopMostService.Service.UnregisterTopMostElement( _autoClickWindow );
                TopMostService.Service.UnregisterTopMostElement( _mouseIndicatorWindow );
            }
        }

        void OnTopMostServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                TopMostService.Service.RegisterTopMostElement( "10", _autoClickWindow );
                TopMostService.Service.RegisterTopMostElement( "200", _mouseIndicatorWindow );
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                TopMostService.Service.UnregisterTopMostElement( _autoClickWindow );
                TopMostService.Service.UnregisterTopMostElement( _mouseIndicatorWindow );
            }
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
            if( e.MultiPluginId.Any( ( c ) => c.UniqueId.Equals( PluginId.UniqueId ) ) && !String.IsNullOrEmpty( e.Key ) )
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
                MouseDriver.Service.PointerMove -= OnPointerMove;
                MouseDriver.Service.PointerButtonDown -= OnPointerButtonDown;
                MouseDriver.Service.PointerButtonUp -= OnPointerButtonUp;
            }
        }

        void OnPointerMove( object sender, PointerDeviceEventArgs e )
        {
            SetPointerPosition( e.X, e.Y );
        }

        void SetPointerPosition( int X, int Y )
        {
            _mouseIndicatorWindow.Left = X + 10;
            _mouseIndicatorWindow.Top = Y - 20;
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
            IsPaused = true;
            OnPropertyChanged( "ProgressValue" );
        }

        private void OnHasResumed( object sender, EventArgs e )
        {
            IsPaused = false;
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

        #region ISharedData

        double _autoClickOpacity;
        public double AutoClickOpacity 
        { 
            get { return _autoClickOpacity; } 
            set
            {
                if( value != _autoClickOpacity )
                {
                    _autoClickOpacity = value;
                    OnPropertyChanged();
                }
            }
        }

        Color _autoClickBackgroundColor;
        public Color AutoClickBackgroundColor 
        { 
            get { return _autoClickBackgroundColor; } 
            set
            {
                if( value != _autoClickBackgroundColor )
                {
                    _autoClickBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }

        int _autoClickBorderThickness;
        public int AutoClickBorderThickness 
        { 
            get { return _autoClickBorderThickness; }
            set
            {
                if( value != _autoClickBorderThickness )
                {
                    _autoClickBorderThickness = value;
                    OnPropertyChanged();
                }
            }
        }

        private Brush _autoClickBorderBrush;
        public Brush AutoClickBorderBrush 
        { 
            get { return _autoClickBorderBrush; }
            set
            {
                if( value != _autoClickBorderBrush )
                {
                    _autoClickBorderBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        void OnSharedPropertyChanged( object sender, SharedPropertyChangedEventArgs e )
        {
            switch( e.PropertyName )
            {
                case "WindowOpacity":
                    AutoClickOpacity = SharedData.Service.WindowOpacity;
                    break;
                case "WindowBorderThickness":
                    AutoClickBorderThickness = SharedData.Service.WindowBorderThickness;
                    break;
                case "WindowBorderBrush":
                    AutoClickBorderBrush = new SolidColorBrush( SharedData.Service.WindowBorderBrush );
                    break;
                case "WindowBackgroundColor":
                    AutoClickBackgroundColor = SharedData.Service.WindowBackgroundColor;
                    break;
            }
        }

        void InitializeSharedData()
        {
            AutoClickOpacity = SharedData.Service.WindowOpacity;
            AutoClickBackgroundColor = SharedData.Service.WindowBackgroundColor;
            AutoClickBorderBrush = new SolidColorBrush( SharedData.Service.WindowBorderBrush );
            AutoClickBorderThickness = SharedData.Service.WindowBorderThickness;
        }

        #endregion

        #region Methods

        private void RegisterEvents()
        {
            MouseDriver.Service.PointerButtonDown += OnPointerButtonDown;
            MouseDriver.Service.PointerButtonUp += OnPointerButtonUp;

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

            SharedData.Service.SharedPropertyChanged += OnSharedPropertyChanged;
        }

        void OnPointerButtonDown( object sender, PointerDeviceEventArgs e )
        {
            MouseIndicatorIsEnable = false;
        }

        void OnPointerButtonUp( object sender, PointerDeviceEventArgs e )
        {
            MouseIndicatorIsEnable = true;
        }

        private void UnregisterEvents()
        {
            if( MouseDriver.Status != InternalRunningStatus.Stopped && MouseDriver.Status != InternalRunningStatus.Disabled )
            {
                MouseDriver.Service.PointerMove -= OnPointerMove;
                MouseDriver.Service.PointerButtonDown -= OnPointerButtonDown;
                MouseDriver.Service.PointerButtonUp -= OnPointerButtonUp;
            }

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