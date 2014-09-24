#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\ClickSelectorService\ClickSelector.cs) is part of CiviKey. 
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
    public class ClickSelector : CK.WPF.ViewModel.VMBase, IClickSelector, IPlugin, IVisualizableHighlightableElement
    {
        const string PluginGuidString = "{1986E566-7426-44DC-ACA3-9E8E8EB673B8}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Click Selector by scroller";
        public readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginIdVersion, PluginPublicName );

        #region Variables & Properties

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ISharedData SharedData { get; set; }

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        private CKReadOnlyCollectionOnICollection<ClickEmbedderVM> _clicksVmReadOnlyAdapter;

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

            InitializeHighlighter();

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

            UninitializeHighlighter();

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
                Highlighter.Service.RegisterTree( "ClickSelector", R.ClickPanel, this );
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
            get { return 0; }
        }

        public int Y
        {
            get { return 0; }
        }

        public int Width
        {
            get { return 0; }
        }

        public int Height
        {
            get { return 0; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.None; }
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

        #region IVizualizableHighlightableElement Members

        public string ElementName
        {
            get { return PluginPublicName; }
        }

        public string VectorImagePath
        {
            get { return "M20.180747,21.585C20.228347,21.644298,20.280347,21.697097,20.326047,21.756895L26.422185,29.758859C29.804953,34.199928,28.94756,40.541042,24.506903,43.923441L23.98421,44.32263C19.541353,47.70653,13.201616,46.847954,9.817549,42.408587L3.7226885,34.40622C3.6758092,34.346324,3.6399197,34.281224,3.5950002,34.220126z M5.408216,20.435999L10.625,27.283507 2.9199337,33.153C0.58399725,28.923633,1.5669129,23.543948,5.408216,20.435999z M12.217023,17.785192C14.812087,17.769212,17.386172,18.751739,19.330999,20.650035L11.625712,26.519999 6.4099998,19.673508C8.1675651,18.414083,10.19864,17.79762,12.217023,17.785192z M7.7603743,0L9.9109099,0C11.445591,1.4389477 12.433944,3.1620827 11.865614,5.1171455 11.457792,6.5197239 10.610847,7.7689734 8.4435108,8.6654558L8.3360851,8.6986551C8.2857921,8.710865 3.4495938,9.8268156 1.8369575,12.91929 1.153331,14.226479 1.1462507,15.741566 1.8142462,17.423152 2.0276377,17.723249 3.2987057,19.358735 5.2883121,18.597541L5.757087,19.825031C3.2381525,20.788423,1.3342407,19.109937,0.67869186,18.088446L0.6232686,17.982246C-0.22443271,15.900264 -0.20734191,13.989581 0.67405343,12.306095 2.4920226,8.8339043 7.363863,7.579525 7.9896266,7.4310856 9.6283839,6.7435923 10.455338,5.9315891 10.556144,5.0248566 10.783956,2.9643345 9.4713957,1.3014984 7.7603743,0z"; }
        }

        #endregion
    }
}
