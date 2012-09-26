#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\VMIPlugin.cs) is part of CiviKey. 
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
using System.Windows.Forms;
using CK.Core;
using CK.Plugin;
using CK.WPF.ViewModel;
using CK.Plugin.Config;
using CK.Plugin.Hosting;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;

namespace CK.Plugins.ObjectExplorer
{
    public class VMIPlugin : VMICoreElement
    {
        IPluginInfo _pluginInfo;
        IList<IPluginInfo> _canEdit;
        PluginRunner _pluginRunner;

        #region Properties

        public bool IsOldVersion { get { return _pluginInfo.IsOldVersion; } }

        public bool IsRunning
        {
            get
            {
                return _pluginRunner.IsPluginRunning( _pluginInfo );
            }
        }

        public string Name { get { return _pluginInfo.PublicName; } }

        public SolvedConfigStatus FinalRequirement 
        { 
            get 
            {
                int finalRes = (int)_pluginRunner.RunnerRequirements.FinalRequirement( _pluginInfo.PluginId );
                int serviceRes = (int)ImplementedServiceStrongestRequirement;
               
                if( finalRes > serviceRes )
                    return (SolvedConfigStatus)Enum.Parse( typeof( SolvedConfigStatus ), finalRes.ToString() );
                return (SolvedConfigStatus)Enum.Parse( typeof( SolvedConfigStatus ), serviceRes.ToString() );
            } 
        }

        public string Description { get { return _pluginInfo.Description; } }

        public Version Version { get { return _pluginInfo.Version; } }

        public string Icon { get { return "../DetailsImages/defaultPluginIcon.png"; } }

        public Guid Id { get { return _pluginInfo.PluginId; } }

        public Uri RefUrl { get { return _pluginInfo.RefUrl; } }

        public SolvedConfigStatus SolvedConfigurationStatus
        {
            get { return _pluginRunner.ConfigManager.SolvedPluginConfiguration.Find( Id ).Status; }
        }

        /// <summary>
        /// Requirements of the implemented service
        /// Shows the way other plugins require the implementedService and therefore requiring the current plugin
        /// Returns null if there is no Implemented Service
        /// Returns an empty collection if there are no running plugins that reference the implemented service
        /// </summary>
        public ICollection<KeyValuePair<VMIPlugin, RunningRequirement>> ImplementedServiceRequirements
        {
            get
            {
                if( ImplService != null ) return ImplService.AllReferencingPlugins;
                return null;
            }
        }

        /// <summary>
        /// Will return the strongest requirement on the implemented service (if any), regarding LAUNCHED plugins that reference it.
        /// Be careful, will return Optional even if there are no service implemented, or if there is no plugins requiring this service.
        /// Make sure you check these two things before showing the result to the user.
        /// </summary>
        public RunningRequirement ImplementedServiceStrongestRequirement
        {
            get
            {
                RunningRequirement req = RunningRequirement.Optional;
                if( ImplementedServiceRequirements != null && !IsImplementedServiceFallbackLaunched)
                {
                    foreach( var item in ImplementedServiceRequirements )
                    {
                        if( _pluginRunner.IsPluginRunning( item.Key._pluginInfo ) && req < item.Value ) req = item.Value;
                    }
                }
                return req;
            }
        }


        public VMIService ImplService
        {
            get
            {
                if( _pluginInfo.Service != null )
                {
                    return VMIContext.FindOrCreateDynamic( _pluginInfo.Service );
                }
                return null;
            }
        }

        /// <summary>
        /// Returns true if the plugin implements a service, that has another plugin implementing it, and that this other plugin is launched.
        /// Returns false otherwise.
        /// </summary>
        public bool IsImplementedServiceFallbackLaunched
        {
            get
            {
                return ImplService != null
                    && ImplService.ImplementedBy.Count > 1
                    && ImplService.ImplementedBy.Where( ( p ) => p.AssemblyFullName != this.AssemblyFullName ).FirstOrDefault( ( p ) => { return _pluginRunner.IsPluginRunning( ( (VMIPlugin)p.Data )._pluginInfo ); } ) != null;
            }
        }

        /// <summary>
        /// Result of the resolution of all Layers (not user, nor system, nor live) 
        /// that have been added throughout the execution (can be the context, the keyboard or the layout for example)
        /// </summary>
        public RunningRequirement StrongestRequirement
        {
            get
            {
                RunningRequirement maxReq = RunningRequirement.Optional;

                foreach( RequirementLayer layer in RequirementLayers )
                {
                    var req = layer.PluginRequirements.Find( _pluginInfo.PluginId );
                    if( req.Requirement > maxReq )
                        maxReq = req.Requirement;
                }

                return maxReq;
            }
        }

        private IEnumerable<RequirementLayer> RequirementLayers
        {
            get { return _pluginRunner.RunnerRequirements.Where( l => l.PluginRequirements.Any( r => r.PluginId == _pluginInfo.PluginId ) ); }
        }

        ICollection<VMIPluginRequirementLayer> _vmRequirementLayers;
        public ICollection<VMIPluginRequirementLayer> VMRequirementLayers
        {
            get
            {
                return _vmRequirementLayers;
            }
        }

        ConfigPluginStatus _systemPluginStatus;
        public ConfigPluginStatus SystemPluginStatus
        {
            get { return _systemPluginStatus; }
            set
            {
                if( value != _systemPluginStatus )
                {
                    _systemPluginStatus = value;
                    VMIContext.Context.ConfigManager.SystemConfiguration.PluginsStatus.SetStatus( Id, value );
                    OnPropertyChanged( "StrongestConfigStatus" );
                }
            }
        }

        ConfigPluginStatus _userPluginStatus;
        public ConfigPluginStatus UserPluginStatus
        {
            get { return _userPluginStatus; }
            set
            {
                if( value != _userPluginStatus )
                {
                    _userPluginStatus = value;
                    VMIContext.Context.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( Id, value );
                    OnPropertyChanged( "StrongestConfigStatus" );
                }
            }
        }

        public ConfigPluginStatus StrongestConfigStatus
        {
            get
            {
                if( UserPluginStatus == ConfigPluginStatus.Disabled || SystemPluginStatus == ConfigPluginStatus.Disabled ) return ConfigPluginStatus.Disabled;
                return UserPluginStatus > SystemPluginStatus ? UserPluginStatus : SystemPluginStatus;
            }
        }

        public ConfigUserAction ConfigUserAction
        {
            get { return _pluginRunner.ConfigManager.UserConfiguration.LiveUserConfiguration.GetAction( _pluginInfo.PluginId ); }
        }

        public IReadOnlyList<string> Categories { get { return _pluginInfo.Categories; } }


        #endregion

        #region Commands

        public VMCommand StartCommand { get; private set; }

        public VMCommand StopCommand { get; private set; }

        public VMCommand ApplyCommand { get; private set; }

        public VMCommand ResetUserAction { get; private set; }

        #endregion

        #region Collections

        public VMCollection<VMAlias<VMIService>, IServiceReferenceInfo> ServiceRefs { get; private set; }
        public VMCollection<VMAlias<VMIPlugin>, IPluginInfo> CanEdit { get; private set; }
        public VMCollection<VMAlias<VMIPlugin>, IPluginInfo> EditableBy { get; private set; }
        public VMCollection<VMAlias<VMIPlugin>, IPluginInfo> RequiredPlugins { get; private set; }
        public VMCollection<VMAlias<VMIPlugin>, IPluginInfo> OptionalPlugins { get; private set; }

        #endregion

        public VMIPlugin( VMIContextViewModel ctx, IPluginInfo plugin, VMIBase parent )
            : base( ctx, parent )
        {
            _pluginInfo = plugin;

            Label = plugin.PublicName;
            Assembly = plugin.AssemblyInfo.AssemblyName;
            DetailsTemplateName = "PluginDetails";

            _systemPluginStatus = VMIContext.Context.ConfigManager.SystemConfiguration.PluginsStatus.GetStatus( Id, ConfigPluginStatus.Manual );
            _userPluginStatus = VMIContext.Context.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( Id, ConfigPluginStatus.Manual );

            _pluginRunner = VMIContext.Context.GetService<PluginRunner>( true );
            _pluginRunner.ApplyDone += new EventHandler<ApplyDoneEventArgs>( OnApplyDone );

            VMIContext.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.Changed += new EventHandler<LiveUserConfigurationChangedEventArgs>( OnLiveUserConfigurationChanged );
            VMIContext.Context.ConfigManager.SystemConfiguration.PluginsStatus.Changed += new EventHandler<PluginStatusCollectionChangedEventArgs>( OnSystemPluginStatusChanged );
            VMIContext.Context.ConfigManager.UserConfiguration.PluginsStatus.Changed += new EventHandler<PluginStatusCollectionChangedEventArgs>( OnUserPluginStatusChanged );

            VMIContext.Context.ConfigManager.UserConfiguration.PropertyChanged += new PropertyChangedEventHandler( OnUserConfigurationChanged );
            VMIContext.Context.ConfigManager.SystemConfiguration.PropertyChanged += new PropertyChangedEventHandler( OnSystemConfigurationChanged );

            CreateCommands();

            IList<IPluginInfo> editableBy = new List<IPluginInfo>();
            foreach( IPluginConfigAccessorInfo p in plugin.EditableBy )
                editableBy.Add( p.Plugin );
            _canEdit = new List<IPluginInfo>();
            foreach( IPluginConfigAccessorInfo p in _pluginInfo.EditorsInfo )
                _canEdit.Add( p.EditedSource );
            IList<IPluginInfo> required = new List<IPluginInfo>();

            _vmRequirementLayers = new List<VMIPluginRequirementLayer>();
            foreach( RequirementLayer layer in RequirementLayers )
            {
                _vmRequirementLayers.Add( new VMIPluginRequirementLayer( this, layer ) );
            }

            ServiceRefs = new VMCollection<VMAlias<VMIService>, IServiceReferenceInfo>( plugin.ServiceReferences, ( info ) => { return new VMAlias<VMIService>( VMIContext.FindOrCreate( info ), this ); } );
            CanEdit = new VMCollection<VMAlias<VMIPlugin>, IPluginInfo>( new ReadOnlyListOnIList<IPluginInfo>( _canEdit ), ( info ) => { return new VMAlias<VMIPlugin>( VMIContext.FindOrCreate( info ), this ); } );
            EditableBy = new VMCollection<VMAlias<VMIPlugin>, IPluginInfo>( new ReadOnlyListOnIList<IPluginInfo>( editableBy ), ( info ) => { return new VMAlias<VMIPlugin>( VMIContext.FindOrCreate( info ), this ); } );
        }

        void Start()
        {
            _pluginRunner.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( _pluginInfo.PluginId, ConfigUserAction.Started );
            _pluginRunner.Apply();
        }

        bool CanStart()
        {
            return !IsRunning;
        }

        void Stop()
        {
            _pluginRunner.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( _pluginInfo.PluginId, ConfigUserAction.Stopped );
            _pluginRunner.Apply();
        }

        bool CanStop()
        {
            return IsRunning;
        }

        void CreateCommands()
        {
            ResetUserAction = new VMCommand( () => VMIContext.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( _pluginInfo.PluginId, Plugin.Config.ConfigUserAction.None ) );

            ApplyCommand = new VMCommand( () => _pluginRunner.Apply() );

            StartCommand = new VMCommand( Start, CanStart );
            StopCommand = new VMCommand( Stop, CanStop );
        }

        #region User Configuration Changes

        void OnUserConfigurationChanged( object o, PropertyChangedEventArgs e )
        {
            RefreshUserPluginStatus();
        }

        void OnUserPluginStatusChanged( object sender, PluginStatusCollectionChangedEventArgs e )
        {
            if( e.PluginID == Id ) RefreshUserPluginStatus();
        }

        void RefreshUserPluginStatus()
        {
            _userPluginStatus = VMIContext.Context.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( Id, ConfigPluginStatus.Manual );
            OnPropertyChanged( "UserPluginStatus" );
        }

        #endregion

        #region System Configuration Changes

        void OnSystemConfigurationChanged( object o, PropertyChangedEventArgs e )
        {
            RefreshSystemPluginStatus();
        }

        void OnSystemPluginStatusChanged( object sender, PluginStatusCollectionChangedEventArgs e )
        {
            if( e.PluginID == Id )
            {
                RefreshSystemPluginStatus();
            }
        }

        private void RefreshSystemPluginStatus()
        {
            _systemPluginStatus = VMIContext.Context.ConfigManager.SystemConfiguration.PluginsStatus.GetStatus( Id, ConfigPluginStatus.Manual );
            OnPropertyChanged( "SystemPluginStatus" );
        }

        #endregion

        void OnLiveUserConfigurationChanged( object sender, LiveUserConfigurationChangedEventArgs e )
        {
            if( e.PluginID == _pluginInfo.PluginId )
            {
                OnPropertyChanged( "ConfigUserAction" );
            }
        }

        void OnApplyDone( object sender, EventArgs e )
        {
            OnPropertyChanged( "IsRunning" );
            OnPropertyChanged( "StrongestRequirement" );
            OnPropertyChanged( "FinalRequirement" );
            OnPropertyChanged( "StrongestConfigStatus" );
        }

        protected override void OnDispose()
        {
            _pluginRunner.ApplyDone -= new EventHandler<ApplyDoneEventArgs>( OnApplyDone );

            VMIContext.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.Changed -= new EventHandler<LiveUserConfigurationChangedEventArgs>( OnLiveUserConfigurationChanged );
            VMIContext.Context.ConfigManager.SystemConfiguration.PluginsStatus.Changed -= new EventHandler<PluginStatusCollectionChangedEventArgs>( OnSystemPluginStatusChanged );
            VMIContext.Context.ConfigManager.UserConfiguration.PluginsStatus.Changed -= new EventHandler<PluginStatusCollectionChangedEventArgs>( OnUserPluginStatusChanged );

            VMIContext.Context.ConfigManager.UserConfiguration.PropertyChanged -= new PropertyChangedEventHandler( OnUserConfigurationChanged );
            VMIContext.Context.ConfigManager.SystemConfiguration.PropertyChanged -= new PropertyChangedEventHandler( OnSystemConfigurationChanged );

            base.OnDispose();
        }
    }

    public class VMIPluginRequirementLayer : VMBase
    {
        RequirementLayer _layer;
        VMIPlugin _plugin;

        /// <summary>
        /// Gets whether this requirement layer has a requirement for the current plugin
        /// </summary>
        public bool HasRequirement
        {
            get { return _layer.PluginRequirements.FirstOrDefault( ( p ) => p.PluginId == _plugin.Id ) != null; }
        }

        /// <summary>
        /// Name of the layer
        /// </summary>
        public String Name { get { return _layer.LayerName; } }

        /// <summary>
        /// Gets the requirement that can be found in the layer
        /// Returns Optional if no requirements can be found
        /// </summary>
        public RunningRequirement Requirement { get { return GetRequirement(); } }

        public VMIPluginRequirementLayer( VMIPlugin plugin, RequirementLayer layer )
        {
            _layer = layer;
            _plugin = plugin;
        }

        private RunningRequirement GetRequirement()
        {
            var req = _layer.PluginRequirements.FirstOrDefault( ( p ) => p.PluginId == _plugin.Id );
            if( req != null ) return req.Requirement;
            return RunningRequirement.Optional;
        }
    }
}
