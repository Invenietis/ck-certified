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

        public string OSName { get { return OSVersionInfo.Name; } }

        public string OSEditionName { get { return OSVersionInfo.Edition; } }

        public string OSServicePack { get { return OSVersionInfo.ServicePack; } }

        public int OSBuildVersion { get { return OSVersionInfo.BuildVersion; } }

        public int OSRevisionVersion { get { return OSVersionInfo.RevisionVersion; } }

        public Version OSVersion { get { return OSVersionInfo.Version; } }

        public string MachineName { get { return Environment.MachineName; } }

        public string UserName { get { return Environment.UserName; } }

        public OSVersionInfo.SoftwareArchitecture OSBits { get { return OSVersionInfo.OSBits; } }

        public OSVersionInfo.ProcessorArchitecture MachineBits { get { return OSVersionInfo.ProcessorBits; } }

        public OSVersionInfo.SoftwareArchitecture SoftBits { get { return OSVersionInfo.ProgramBits; } }
    }
}
