
using CK.Core;
using System;
using System.Collections.Generic;
using System.Windows;
using CK.Windows.App;
namespace CK.Plugins.ObjectExplorer
{
    public class VMOSInfo : VMISelectableElement
    {
        public VMOSInfo( VMIContextViewModel ctx )
            : base( ctx, null )
        {
        }

        public string SystemConfigurationPath { get { return System.IO.Path.Combine(CKApp.CurrentParameters.CommonApplicationDataPath, "System.config.ck"); } }

        public string UserConfigurationPath { get { return System.IO.Path.Combine(CKApp.CurrentParameters.ApplicationDataPath, "User.config.ck"); } }

        public string ContextPath { get { return System.IO.Path.Combine(CKApp.CurrentParameters.ApplicationDataPath, "Context.xml"); } }        

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
