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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugins.AutoClick.ViewModel;
using CK.Plugins.AutoClick.Model;
using CK.Plugins.AutoClick.Views;
using System.Windows.Input;
using CK.WPF.ViewModel;
using System.Diagnostics;
using System.ComponentModel;
using CK.Plugin;
using CK.Core;
using CommonServices.Accessibility;
using HighlightModel;
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
        public IService<IWindowBinder> WindowBinder { get; set; }

        private CKReadOnlyCollectionOnICollection<ClickEmbedderVM> _clicksVmReadOnlyAdapter;
        private ClickSelectorWindow _clickSelectorWindow;

        public ClicksVM ClicksVM { get; set; }
        public ICKReadOnlyList<ClickEmbedderVM> ReadOnlyClicksVM { get { return _clicksVmReadOnlyAdapter.ToReadOnlyList(); } }

        #endregion

        #region IPlugin members

        public bool Setup( IPluginSetupInfo info )
        {
            ClicksVM = new ClicksVM();
            _clicksVmReadOnlyAdapter = new CKReadOnlyCollectionOnICollection<ClickEmbedderVM>( ClicksVM );
            return true;
        }

        public void Start()
        {
            if( Highlighter.Status.IsStartingOrStarted )
            {
                RegisterHighlighterService();
            }

            Highlighter.ServiceStatusChanged += Highlighter_ServiceStatusChanged;

            _clickSelectorWindow = new ClickSelectorWindow() { DataContext = this };
            _clickSelectorWindow.Show();

            WindowManager.Service.RegisterWindow( "ClickSelector", _clickSelectorWindow );
        }

        void Highlighter_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
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

        public void Stop()
        {
            UnregisterHighlighterService();

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

            if( AutoClickClickTypeChosen != null && clickToLaunch != null )
            {
                AutoClickClickTypeChosen( this, new ClickTypeEventArgs( clickToLaunch ) );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event fired when a click has been chosen
        /// </summary>
        public event ClickTypeChosenEventHandler AutoClickClickTypeChosen;

        /// <summary>
        /// Event fired when the AutoClickPlugin should be stopped
        /// </summary>
        public event AutoClickStopEventHandler AutoClickStopEvent;

        /// <summary>
        /// Event fired when the AutoclickPlugin may go back to work
        /// </summary>
        public event AutoClickResumeEventHandler AutoClickResumeEvent;

        #endregion

        #endregion

        #region IHighlightable Members

        private void RegisterHighlighterService()
        {
            Highlighter.Service.RegisterTree( this );
            Highlighter.Service.BeginHighlight += OnBeginHighlight;
            Highlighter.Service.EndHighlight += OnEndHighlight;
            Highlighter.Service.SelectElement += OnScrollerSelect;
        }

        private void UnregisterHighlighterService()
        {
            Highlighter.Service.BeginHighlight -= OnBeginHighlight;
            Highlighter.Service.EndHighlight -= OnEndHighlight;
            Highlighter.Service.SelectElement -= OnScrollerSelect;
            Highlighter.Service.UnregisterTree( this );
        }

        bool _isHighlighted;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;
                OnPropertyChanged( "IsHighlighted" );
                foreach( var click in ClicksVM )
                {
                    click.IsHighlighted = value;
                }
            }
        }

        private void OnBeginHighlight( object sender, HighlightEventArgs e )
        {
            if( e.Element == this )
            {
                IsHighlighted = true;
            }
            else if( e.Element is ClickEmbedderVM )
            {
                var result = ClicksVM.SingleOrDefault( ( el ) => el == e.Element );
                Debug.Assert( result != null, "BeginHighlight was called on a ClickType that does not exist" );
                result.IsHighlighted = true;
            }
        }

        private void OnEndHighlight( object sender, HighlightEventArgs e )
        {
            if( e.Element is ClickSelector )
            {
                IsHighlighted = false;
            }
            else if( e.Element is ClickEmbedderVM )
            {
                ClickEmbedderVM clickEmbedder = ClicksVM.SingleOrDefault( ( el ) => el == e.Element );
                Debug.Assert( clickEmbedder != null, "EndHighlight was called on a ClickType that does not exist" );
                clickEmbedder.IsHighlighted = false;
            }
        }

        void OnScrollerSelect( object sender, HighlightEventArgs e )
        {
            if( e.Element is ClickEmbedderVM )
            {
                ClickEmbedderVM clickEmbedder = ClicksVM.SingleOrDefault( ( el ) => el == e.Element );
                Debug.Assert( clickEmbedder != null, "SelectElement was called on a ClickType that does not exist" );
                clickEmbedder.DoSelect();
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
    }
}
