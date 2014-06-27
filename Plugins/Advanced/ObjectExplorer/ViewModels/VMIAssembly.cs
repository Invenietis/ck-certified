#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\VMIAssembly.cs) is part of CiviKey. 
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
