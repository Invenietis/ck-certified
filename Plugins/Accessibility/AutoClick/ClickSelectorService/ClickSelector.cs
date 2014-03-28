#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\ClickSelector.cs) is part of CiviKey. 
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
using CK.Plugins.AutoClick.ViewModel;
using CK.Plugins.AutoClick.Views;
using CK.Plugin;
using CK.Core;
using CommonServices.Accessibility;
using HighlightModel;
using CK.Plugin.Config;
using CK.Windows;
using CK.WindowManager.Model;

namespace CK.Plugins.AutoClick
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class ClickSelector : CK.WPF.ViewModel.VMBase, IClickSelector, IPlugin, IHighlightableElement
    {
        const string PluginGuidString = "{F9687F04-7370-4812-9EB4-1320EB282DD8}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Click Selector";
        public readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginIdVersion, PluginPublicName );

        #region Variables & Properties

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ITopMostService> TopMostService { get; set; }

        private CKReadOnlyCollectionOnICollection<ClickEmbedderVM> _clicksVmReadOnlyAdapter;
        private ClickSelectorWindow _clickSelectorWindow;

        public IPluginConfigAccessor Config { get; set; }

        public ClicksVM ClicksVM { get; set; }
        public ICKReadOnlyList<ClickEmbedderVM> ReadOnlyClicksVM { get { return _clicksVmReadOnlyAdapter.ToReadOnlyList(); } }

        #endregion

        #region IPlugin members

        public bool Setup( IPluginSetupInfo info )
        {
            ClicksVM = new ClicksVM() { Holder = this };
            _clicksVmReadOnlyAdapter = new CKReadOnlyCollectionOnICollection<ClickEmbedderVM>( ClicksVM );
            return true;
        }

        public void Start()
        {

            int defaultHeight = (int)( System.Windows.SystemParameters.WorkArea.Width ) / 4;
            int defaultWidth = defaultHeight / 4;

            _clickSelectorWindow = new ClickSelectorWindow() { DataContext = this };

            InitializeHighlighter();
            InitializeWindowManager();
            InitializeTopMost();

            if( !Config.User.Contains( "ClickSelectorWindowPlacement" ) )
            {
                SetDefaultWindowPosition( defaultWidth, defaultHeight );
            }
            else
            {
                _clickSelectorWindow.Width = _clickSelectorWindow.Height = 0;
            }

            _clickSelectorWindow.Show();

            //Executed only at first launch, has to be done once the window is shown, otherwise, it will save a "hidden" state for the window
            if( !Config.User.Contains( "ClickSelectorWindowPlacement" ) ) Config.User.Set( "ClickSelectorWindowPlacement", CKWindowTools.GetPlacement( _clickSelectorWindow.Hwnd ) );
            CKWindowTools.SetPlacement( _clickSelectorWindow.Hwnd, (WINDOWPLACEMENT)Config.User["ClickSelectorWindowPlacement"] );
        }

        private void SetDefaultWindowPosition( int defaultWidth, int defaultHeight )
        {
            _clickSelectorWindow.Top = 100;
            _clickSelectorWindow.Left = (int)System.Windows.SystemParameters.WorkArea.Width - defaultWidth;
            _clickSelectorWindow.Width = defaultWidth;
            _clickSelectorWindow.Height = defaultHeight;
        }

        public void Stop()
        {
            TopMostService.Service.UnregisterTopMostElement( _clickSelectorWindow );

            UninitializeTopMost();
            UninitializeWindowManager();
            UninitializeHighlighter();

            Config.User.Set( "ClickSelectorWindowPlacement", CKWindowTools.GetPlacement( _clickSelectorWindow.Hwnd ) );

            _clickSelectorWindow.Close();
            _clickSelectorWindow = null;
        }

        public void Teardown()
        {
            ClicksVM = null;
        }

        #endregion

        #region IClickSelector Members

        #region Methods

        /// <summary>
        /// Part of the IClickTypeSelector Interface, called by the AutoClickPlugin when it needs to launch a click.
        /// This method doesn't return the Click as it may need to be asyncronous (when the PointerClickTypeSelector is enabled)
        /// </summary>
        public void AskClickType()
        {
            ClickVM clickToLaunch = null;

            clickToLaunch = ClicksVM.GetNextClick( true );

            if( ClickChosen != null && clickToLaunch != null )
            {
                ClickChosen( this, new ClickTypeEventArgs( clickToLaunch ) );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event fired when a click has been chosen
        /// </summary>
        public event ClickChosenEventHandler ClickChosen;

        /// <summary>
        /// Event fired when the AutoClickPlugin should be stopped
        /// </summary>
        public event EventHandler StopEvent;

        /// <summary>
        /// Event fired when the AutoclickPlugin may go back to work
        /// </summary>
        public event EventHandler ResumeEvent;

        #endregion

        #endregion

        #region IHighlightable Members

        void InitializeHighlighter()
        {
            RegisterHighlighterService();
            Highlighter.ServiceStatusChanged += OnHighlighterStatusChanged;
        }
        void UninitializeHighlighter()
        {
            Highlighter.ServiceStatusChanged -= OnHighlighterStatusChanged;
            UnregisterHighlighterService();
        }

        void OnHighlighterStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                RegisterHighlighterService();
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                UnregisterHighlighterService();
            }
        }

        void RegisterHighlighterService()
        {
            if( Highlighter.Status.IsStartingOrStarted )
            {
                Highlighter.Service.RegisterTree( "ClickSelector", this );
            }
        }

        void UnregisterHighlighterService()
        {
            if( Highlighter.Status.IsStartingOrStarted )
            {
                Highlighter.Service.UnregisterTree( "ClickSelector", this );
            }
        }

        bool _isHighlighted;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;
                OnPropertyChanged( "IsHighlighted" );

                if( ClicksVM == null ) return; //May occur when the scroller triggers the EnnHighlight after the plugin has been stopped

                foreach( var click in ClicksVM )
                {
                    click.IsHighlighted = value;
                }
            }
        }

        public ICKReadOnlyList<IHighlightableElement> Children { get { return ReadOnlyClicksVM; } }

        public int X
        {
            get { return (int)_clickSelectorWindow.Left; }
        }

        public int Y
        {
            get { return (int)_clickSelectorWindow.Top; }
        }

        public int Width
        {
            get { return (int)_clickSelectorWindow.Width; }
        }

        public int Height
        {
            get { return (int)_clickSelectorWindow.Height; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.None; }
        }

        #endregion

        #region IWindowManager Members

        void InitializeWindowManager()
        {
            RegisterWindowManager();

            //Register WindowsManager events
            WindowManager.ServiceStatusChanged += OnWindowManagerStatusChanged;
            if( WindowManager.Status.IsStartingOrStarted )
            {
                WindowManager.Service.WindowMinimized += OnWindowMinimized;
                WindowManager.Service.WindowRestored += OnWindowRestored;
            }
        }

        void UninitializeWindowManager()
        {
            if( WindowManager.Status.IsStartingOrStarted )
            {
                WindowManager.Service.WindowMinimized -= OnWindowMinimized;
                WindowManager.Service.WindowRestored -= OnWindowRestored;
            }
            WindowManager.ServiceStatusChanged -= OnWindowManagerStatusChanged;

            UnregisterWindowManager();
        }

        void OnWindowRestored( object sender, WindowElementEventArgs e )
        {
            if( e.Window.Window == _clickSelectorWindow ) RegisterHighlighterService();
        }

        void OnWindowMinimized( object sender, WindowElementEventArgs e )
        {
            if( e.Window.Window == _clickSelectorWindow ) UnregisterHighlighterService();
        }

        void OnWindowManagerStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                WindowManager.Service.WindowMinimized += OnWindowMinimized;
                WindowManager.Service.WindowRestored += OnWindowRestored;
                WindowManager.Service.RegisterWindow( "ClickSelector", _clickSelectorWindow );
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                WindowManager.Service.UnregisterWindow( "ClickSelector" );
            }
            else if( e.Current <= InternalRunningStatus.Stopped )
            {
                WindowManager.Service.WindowMinimized -= OnWindowMinimized;
                WindowManager.Service.WindowRestored -= OnWindowRestored;
            }
        }

        void RegisterWindowManager()
        {
            if( WindowManager.Status.IsStartingOrStarted ) WindowManager.Service.RegisterWindow( "ClickSelector", _clickSelectorWindow );
        }

        void UnregisterWindowManager()
        {
            if( WindowManager.Status.IsStartingOrStarted ) WindowManager.Service.UnregisterWindow( "ClickSelector" );
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
            if( TopMostService.Status.IsStartingOrStarted ) TopMostService.Service.RegisterTopMostElement( "10", _clickSelectorWindow );
        }

        void UnregisterTopMost()
        {
            if( TopMostService.Status.IsStartingOrStarted ) TopMostService.Service.UnregisterTopMostElement( _clickSelectorWindow );
        }

        void OnTopMostServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started ) TopMostService.Service.RegisterTopMostElement( "10", _clickSelectorWindow );
            else if( e.Current == InternalRunningStatus.Stopping ) TopMostService.Service.UnregisterTopMostElement( _clickSelectorWindow );
        }


        #endregion

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            IsHighlighted = true;
            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            IsHighlighted = false;
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            scrollingDirective.NextActionType = ActionType.EnterChild;
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return true; }
        }
    }
}
