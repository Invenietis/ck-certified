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
using AutoClick.Res;
using System.ComponentModel;
using CommonServices;

namespace CK.Plugins.AutoClick
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class ClickSelectorByHover : CK.WPF.ViewModel.VMBase, IClickSelector, IPlugin
    {
        const string PluginGuidString = "{F9687F04-7370-4812-9EB4-1320EB282DD8}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Click Selector by hover";
        public readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginIdVersion, PluginPublicName );

        #region Variables & Properties

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ISharedData SharedData { get; set; }

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
            _isClosing = false;
            return true;
        }

        public void Start()
        {
            ClicksVM = new ClicksVM( this, SharedData );
            _clicksVmReadOnlyAdapter = new CKReadOnlyCollectionOnICollection<ClickEmbedderVM>( ClicksVM );
            _clickSelectorWindow = new ClickSelectorWindow() { DataContext = this };
            _clickSelectorWindow.Closing += OnWindowClosing;

            InitializeWindowManager();
            InitializeTopMost();

            _clickSelectorWindow.Show();
        }

        bool _isClosing;
        void OnWindowClosing( object sender, CancelEventArgs e )
        {
            if( !_isClosing )
                e.Cancel = true;
        }

        public void Stop()
        {
            _isClosing = true;

            TopMostService.Service.UnregisterTopMostElement( _clickSelectorWindow );

            UninitializeTopMost();
            UninitializeWindowManager();

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
        public void AskClickType(ClickEmbedderVM click = null)
        {
            ClickVM clickToLaunch = null;

            clickToLaunch = ClicksVM.GetNextClick( true, click );

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

        #region IWindowManager Members

        void InitializeWindowManager()
        {
            RegisterWindowManager();

            //Register WindowsManager events
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
                WindowManager.Service.RegisterWindow( "ClickSelector", _clickSelectorWindow );
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                WindowManager.Service.UnregisterWindow( "ClickSelector" );
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

    }
}
