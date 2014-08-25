#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\VMOsInfo.cs) is part of CiviKey. 
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

using CK.Core;
using System;

namespace CK.Plugins.ObjectExplorer
{
    public class VMOSInfo : VMISelectableElement
    {
        public VMOSInfo( VMIContextViewModel ctx )
            : base( ctx, null )
        {
        }
        public object Data { get { return this; } }

        public string OSName { get { return OSVersionInfo.OSLevelDisplayName; } }

        public string OSEditionName { get { return OSVersionInfo.OSLevelDisplayName; } }

        public string OSServicePack { get { return OSVersionInfo.OSVersion.ServicePack; } }

        public int OSBuildVersion { get { return OSVersion.Build; } }

        public int OSRevisionVersion { get { return OSVersion.Revision; } }

        public Version OSVersion { get { return OSVersionInfo.OSVersion.Version; } }

        public string MachineName { get { return Environment.MachineName; } }

        public string UserName { get { return Environment.UserName; } }

        public OSVersionInfo.SoftwareArchitecture OSBits { get { return OSVersionInfo.OSBits; } }

        public OSVersionInfo.ProcessorArchitecture MachineBits { get { return OSVersionInfo.ProcessorBits; } }

        public OSVersionInfo.SoftwareArchitecture SoftBits { get { return OSVersionInfo.ProcessBits; } }
    }
}
