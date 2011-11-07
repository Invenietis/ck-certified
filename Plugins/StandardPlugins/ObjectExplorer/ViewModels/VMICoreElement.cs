using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using System.Reflection;

namespace CK.StandardPlugins.ObjectExplorer
{
    public class VMICoreElement : VMISelectableElement
    {
        public bool OnError { get; protected set; }

        public AssemblyName Assembly { get; protected set; }

        public string AssemblyFullName { get { return Assembly != null ? Assembly.FullName : String.Empty; } }

        public string AssemblyName { get { return Assembly != null ? Assembly.Name : String.Empty; } }

        public Version AssemblyVersion { get { return Assembly != null ? Assembly.Version : new Version(); } }

        public string AssemblyPath { get { return Assembly != null ? Assembly.CodeBase : String.Empty; } }

        public virtual object Data { get { return this; } }

        public VMICoreElement( VMIContextViewModel ctx, VMIBase parent )
            : base( ctx, parent )
        {
        }
    }
}
