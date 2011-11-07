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

namespace CK.StandardPlugins.ObjectExplorer
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

        public SolvedConfigStatus FinalRequirement { get { return _pluginRunner.RunnerRequirements.FinalRequirement( _pluginInfo.PluginId ); } }

        public string Description { get { return _pluginInfo.Description; } }

        public Version Version { get { return _pluginInfo.Version; } }

        public string Icon { get { return "../DetailsImages/defaultPluginIcon.png"; } }

        public Guid Id { get { return _pluginInfo.PluginId; } }

        public Uri RefUrl { get { return _pluginInfo.RefUrl; } }

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

        public IEnumerable<RequirementLayer> RequirementLayers
        {
            get { return _pluginRunner.RunnerRequirements.Where( l => l.PluginRequirements.Any( r => r.PluginId == _pluginInfo.PluginId ) ); }
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
            get { return _pluginRunner.ConfigManager.UserConfiguration.LiveUserConfiguration.GetAction(_pluginInfo.PluginId); }
        }

        public IReadOnlyList<string> Categories { get { return _pluginInfo.Categories; } }

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

            CreateCommands();

            IList<IPluginInfo> editableBy = new List<IPluginInfo>();
            foreach( IPluginConfigAccessorInfo p in plugin.EditableBy )
                editableBy.Add( p.Plugin );
            _canEdit = new List<IPluginInfo>();
            foreach( IPluginConfigAccessorInfo p in _pluginInfo.EditorsInfo )
                _canEdit.Add( p.EditedSource );
            IList<IPluginInfo> required = new List<IPluginInfo>();
      
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

            ApplyCommand = new VMCommand( () => _pluginRunner.Apply());

            StartCommand = new VMCommand( Start, CanStart );
            StopCommand = new VMCommand( Stop, CanStop );            
        }

        void OnUserPluginStatusChanged( object sender, PluginStatusCollectionChangedEventArgs e )
        {
            if( e.PluginID == Id )
            {
                _userPluginStatus = VMIContext.Context.ConfigManager.UserConfiguration.PluginsStatus.GetStatus( Id, ConfigPluginStatus.Manual );
                OnPropertyChanged( "UserPluginStatus" );
            }
        }

        void OnSystemPluginStatusChanged( object sender, PluginStatusCollectionChangedEventArgs e )
        {
            if( e.PluginID == Id )
            {
                _systemPluginStatus = VMIContext.Context.ConfigManager.SystemConfiguration.PluginsStatus.GetStatus( Id, ConfigPluginStatus.Manual );
                OnPropertyChanged( "SystemPluginStatus" );
            }
        }

        void OnLiveUserConfigurationChanged( object sender, LiveUserConfigurationChangedEventArgs e )
        {
            if( e.PluginID == _pluginInfo.PluginId )
            {
                OnPropertyChanged( "ConfigUserAction" );
            }
        }

        void OnApplyDone(object sender, EventArgs e)
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

            base.OnDispose();
        }
    }

}
