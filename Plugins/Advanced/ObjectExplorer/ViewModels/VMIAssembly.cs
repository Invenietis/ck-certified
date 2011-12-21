using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.WPF.ViewModel;

namespace CK.Plugins.ObjectExplorer
{
    public class VMIAssembly : VMICoreElement
    {
        IAssemblyInfo _assemblyInfo;

        public VMCollection<VMAlias<VMIPlugin>, IPluginInfo> Plugins { get; private set; }

        public VMCollection<VMAlias<VMIService>, IServiceInfo> Services { get; private set; }

        public string Icon { get { return "../DetailsImages/defaultAssemblyIcon.png"; } }

        public VMIAssembly( VMIContextViewModel ctx, IAssemblyInfo assembly )
            : base( ctx, null )
        {
            _assemblyInfo = assembly;
            Assembly = assembly.AssemblyName;
            if( !assembly.HasError )
                Label = assembly.AssemblyName.Name;
            else
                Label = assembly.AssemblyFileName;
            OnError = assembly.HasError;

            DetailsTemplateName = "AssemblyDetails";

            Plugins = new VMCollection<VMAlias<VMIPlugin>, IPluginInfo>( assembly.Plugins, ( info ) => { return new VMAlias<VMIPlugin>( VMIContext.FindOrCreate( info ), this ); } );
            Services = new VMCollection<VMAlias<VMIService>, IServiceInfo>( assembly.Services, ( info ) => { return new VMAlias<VMIService>( VMIContext.FindOrCreate( info ), this ); } );
        }
    }
}
