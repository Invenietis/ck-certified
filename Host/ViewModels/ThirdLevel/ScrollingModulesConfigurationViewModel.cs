#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\ThirdLevel\ScrollingModulesConfigurationViewModel.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Storage;
using CK.Windows;
using CK.Windows.App;
using CK.Windows.Config;
using CommonServices.Accessibility;
using HighlightModel;
using Host.Resources;

namespace Host.VM
{
    public class ScrollingModulesConfigurationViewModel : ConfigBase
    {
        ObservableCollection<ScrollingElement> _modulesFromConfig = new ObservableCollection<ScrollingElement>();
        ObservableCollection<ScrollingElement> _modulesFromService = new ObservableCollection<ScrollingElement>();

        ConfigGroup _fromService;
        ConfigGroup _fromConfig;

        IsDirtyConfigItemApply _applyButton;

        public ScrollingModulesConfigurationViewModel( string displayName, AppViewModel app )
            : base( "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}", "Scrolling", app )
        {
            DisplayName = displayName;
        }

        const string groupName = "CursorPointing";
        bool registered = false;
        protected override void OnActivate()
        {
            base.OnActivate();
            UpdateAll();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            InitializeUI();

            _modulesFromConfig.CollectionChanged += OnCollectionChanged;
            _modulesFromService.CollectionChanged += OnCollectionChanged;

            UpdateAll();
        }

        private void InitializeUI()
        {
            if( _fromService == null && _fromConfig == null )
            {
                var s = AddActivableSection( R.Scrolling, String.Empty );

                if( _fromService == null )
                {
                    _fromService = new ScrollingConfigGroup( _app.ConfigManager );
                    s.Items.Add( _fromService );
                    _fromService.Description = R.ActiveModules;
                    _fromService.DisplayName = R.ScrollableElements;
                }

                if( _fromConfig == null )
                {
                    _fromConfig = new ScrollingConfigGroup( _app.ConfigManager );
                    s.Items.Add( _fromConfig );
                    _fromConfig.Description = R.ForbiddenModules;
                    _fromConfig.DisplayName = R.UnScrollableElements;
                }

                _applyButton = new IsDirtyConfigItemApply( ConfigManager, new VMCommand( Apply ), () => { return IsDirty; } ) { DisplayName = R.Apply };
                this.Items.Add( _applyButton );
            }
        }

        protected override void OnPluginLoaded()
        {
            UpdateAll();
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e != null && e.Key == "ScrollableModules" )
            {
                UpdateAll();
            }
        }

        private void UpdateAll()
        {
            if( _plugin != null && _plugin.IsLoaded )
            {
                GetFromService();
                FillGroupFromModules( ref _fromService, _modulesFromService );
            }

            GetFromConfig();
            FillGroupFromModules( ref _fromConfig, _modulesFromConfig );

            IsDirty = false;
        }

        private void OnElementRegisteredOrUnregistered( object sender, HighlightElementRegisterEventArgs e )
        {
            UpdateAll();
        }

        private void OnCollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            FillGroupFromModules( ref _fromService, _modulesFromService );
            FillGroupFromModules( ref _fromConfig, _modulesFromConfig );
        }

        bool _isDirty;
        internal bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;
                if( _applyButton != null ) _applyButton.UpdateIsEnabled();
            }
        }

        private void Apply()
        {
            Config.Set( "ScrollableModules", new ScrollingElementConfiguration( _modulesFromConfig ) );
        }

        private void FillGroupFromModules( ref ConfigGroup group, ObservableCollection<ScrollingElement> modules )
        {
            if( group == null )
            {
                InitializeUI();
            }

            group.Items.Clear();

            foreach( var module in modules )
            {
                group.Items.Add( module );
            }
        }

        private void GetFromService()
        {
            Debug.Assert( _plugin != null && _plugin.IsLoaded );

            _modulesFromService.Clear();
            //Filling the modules from service, getting every registered elements, regardless of its state.
            IService<IHighlighterService> service = _app.PluginRunner.ServiceHost.GetProxy<IHighlighterService>();
            if( service != null && service.Status.IsStartingOrStarted )
            {
                foreach( var item in service.Service.RegisteredElements )
                {
                    if( !_modulesFromService.Select( m => m.InternalName ).Contains( item.Key ) )
                    {
                        var el = new ScrollingElement( item.Key, item.Value, new VMCommand( () => FromServiceToConf( item.Key ) ), R.ForbidScrolling );
                        _modulesFromService.Add( el );
                    }
                }

                if( !registered )
                {
                    registered = true;
                    service.Service.ElementRegisteredOrUnregistered += OnElementRegisteredOrUnregistered;
                }
            }
        }

        /// <summary>
        /// Action called to forbidden scrolling on a module that is registered in the service and that has the right to be scrolled 
        /// </summary>
        /// <param name="elementInternalName">The internal name of the module to transfert</param>
        internal void FromServiceToConf( string elementInternalName )
        {
            Debug.Assert( _modulesFromService.Any( m => m.InternalName == elementInternalName ) );
            Debug.Assert( !_modulesFromConfig.Any( m => m.InternalName == elementInternalName ) );

            var item = _modulesFromService.Where( e => e.InternalName == elementInternalName ).Single();

            item.Command = item.Command = new VMCommand( () => RemoveFromConf( item.InternalName ) );
            item.CommandDescription = R.EnableScrolling;

            _modulesFromService.Remove( item );
            _modulesFromConfig.Add( item );

            IsDirty = true;
        }

        /// <summary>
        /// Action called to re-enable a module which has been forbidden its right to be scrolled.
        /// If the module is currently registered, it will be transfered into the service collection.
        /// If not, it will be discarded.
        /// </summary>
        /// <param name="elementInternalName">The internal name of the module to transfert</param>
        internal void RemoveFromConf( string elementInternalName )
        {
            Debug.Assert( !_modulesFromService.Any( m => m.InternalName == elementInternalName ) );
            Debug.Assert( _modulesFromConfig.Any( m => m.InternalName == elementInternalName ) );

            var item = _modulesFromConfig.Where( e => e.InternalName == elementInternalName ).Single();

            _modulesFromConfig.Remove( item );

            IService<IHighlighterService> service = _app.PluginRunner.ServiceHost.GetProxy<IHighlighterService>();
            if( service != null && service.Status.IsStartingOrStarted )
            {
                if( service.Service.RegisteredElements.ContainsKey( item.InternalName ) )
                {
                    item.Command = new VMCommand( () => FromServiceToConf( item.InternalName ) );
                    item.CommandDescription = R.ForbidScrolling;
                    _modulesFromService.Add( item );
                }
            }

            IsDirty = true;
        }

        private void GetFromConfig()
        {
            //Filling the modules from config. All elements from this are removed from the service list (they are disabled)
            var configuration = new List<ScrollingElement>();
            if( Config != null ) configuration = Config.GetOrSet( "ScrollableModules", configuration );

            _modulesFromConfig.Clear();

            foreach( var scrollingElement in configuration )
            {
                //If the element is found in the service list, we remove it (it has been found in the conf, which means it is disabled)
                if( _modulesFromService.Any( m => m.InternalName == scrollingElement.InternalName ) )
                {
                    _modulesFromService.Remove( _modulesFromService.Where( s => s.InternalName == scrollingElement.InternalName ).Single() );
                }

                if( !_modulesFromConfig.Contains( scrollingElement ) )
                {
                    scrollingElement.Command = new VMCommand( () => RemoveFromConf( scrollingElement.InternalName ) );
                    scrollingElement.CommandDescription = R.EnableScrolling;

                    _modulesFromConfig.Add( scrollingElement );
                }
            }
        }
    }

    public class ScrollingConfigGroup : ConfigGroup
    {
        public ScrollingConfigGroup( ConfigManager configManager )
            : base( configManager )
        {

        }
    }
}
