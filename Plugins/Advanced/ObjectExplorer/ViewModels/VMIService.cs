using CK.Plugin;
using CK.Plugins.ObjectExplorer.ViewModels.LogViewModels;
using CK.WPF.ViewModel;
using CK.Plugin.Hosting;
using System.Linq;
using System.Collections.Generic;
using CK.Core;
using System;

namespace CK.Plugins.ObjectExplorer
{
    public class VMIService : VMICoreElement
    {
        PluginRunner _pluginRunner;
        IServiceInfo _service;
        IServiceReferenceInfo _serviceRef;
        VMIPlugin _owner;
        VMIService _reference;
        Dictionary<VMIPlugin,RunningRequirement> _allReferencingPlugins;

        public VMLogServiceConfig VMLogServiceConfig { get { return VMIContext.VMLogConfig.Find(this.ServiceFullName); } } 

        public string ServiceFullName { get {return _service.ServiceFullName; } }

        public string AssembyQualifiedName { get { return _service.AssemblyQualifiedName; } }

        public string AssemblyInfo { get { return _service.AssemblyInfo.AssemblyName.Name; } }

        public string Icon { get { return "../DetailsImages/defaultServiceIcon.png"; } }

        public string ErrorMessage { get; set; }

        public VMIPlugin Owner { get { return _owner; } }

        public VMIService Reference { get { return _reference; } }

        public RunningRequirement Requirements { get { return _serviceRef.Requirements; } }

        public string PropertyName { get { return _serviceRef.PropertyName; } }

        public VMCollection<VMAlias<VMIPlugin>, IPluginInfo> ImplementedBy { get; private set; }

        public Dictionary<VMIPlugin, RunningRequirement> AllReferencingPlugins { get { return _allReferencingPlugins; } }        

        public VMIService( VMIContextViewModel ctx, IServiceInfo service, VMIBase parent )
            : base( ctx, parent )
        {
            _service = service;
            Label = service.ServiceFullName;
            if( !service.HasError && service.Implementations.Count == 0 )
                ErrorMessage = "No implementation";
            else
                ErrorMessage = _service.ErrorMessage;
            OnError = ErrorMessage != null;

            if( service.IsDynamicService )
                Assembly = service.AssemblyInfo.AssemblyName;
            
            DetailsTemplateName = "ServiceDetails";

            _pluginRunner = VMIContext.Context.GetService<PluginRunner>( true );
            _pluginRunner.ApplyDone += new EventHandler<ApplyDoneEventArgs>( OnApplyDone );

            _allReferencingPlugins = new Dictionary<VMIPlugin, RunningRequirement>();
            ImplementedBy = new VMCollection<VMAlias<VMIPlugin>, IPluginInfo>( _service.Implementations, ( info ) => { return new VMAlias<VMIPlugin>( VMIContext.FindOrCreate( info ), null ); } );
        }

        public VMIService( VMIContextViewModel ctx, IServiceReferenceInfo service, VMIBase parent )
            : base( ctx, parent )
        {
            _serviceRef = service;
            _service = service.Reference;
            Label = service.Reference.ServiceFullName;
            OnError = service.HasError;            

            if( service.Reference.IsDynamicService )
                Assembly = service.Reference.AssemblyInfo.AssemblyName;

            _pluginRunner = VMIContext.Context.GetService<PluginRunner>( true );
            _pluginRunner.ApplyDone += new EventHandler<ApplyDoneEventArgs>( OnApplyDone );

            DetailsTemplateName = "ServiceRefDetails";
            _owner = new VMIPlugin( ctx, service.Owner, this );
            _reference = new VMIService( ctx, service.Reference, this );

            _allReferencingPlugins = new Dictionary<VMIPlugin,RunningRequirement>();
            ImplementedBy = new VMCollection<VMAlias<VMIPlugin>, IPluginInfo>( _service.Implementations, ( info ) => { return new VMAlias<VMIPlugin>( VMIContext.FindOrCreate( info ), this ); } );
        }

        void OnApplyDone( object sender, EventArgs e )
        {
            RefreshReferenceInfos();
            OnPropertyChanged( "AllReferencingPlugins" );
        }

        void RefreshReferenceInfos()
        {
            RunningRequirement req;
            VMIPlugin vmP;

            foreach( IPluginInfo p in _pluginRunner.Discoverer.AllPlugins )
            {                
                foreach( IServiceReferenceInfo s in p.ServiceReferences )
                {
                    if( s.Reference.AssemblyQualifiedName == _service.AssemblyQualifiedName )
                    {
                        vmP = VMIContext.FindOrCreate( p );
                        if( vmP != null )
                        {
                            if( _allReferencingPlugins.TryGetValue( vmP, out req ) )
                            {
                                if( req != s.Requirements ) _allReferencingPlugins[vmP] = s.Requirements;
                            }
                            else
                            {
                                _allReferencingPlugins.Add( VMIContext.FindOrCreate( p ), s.Requirements );
                            }
                        }
                    }
                }
            }
        }

        protected override void OnDispose()
        {
            _pluginRunner.ApplyDone -= new EventHandler<ApplyDoneEventArgs>( OnApplyDone );

            base.OnDispose();
        }
    }
}
